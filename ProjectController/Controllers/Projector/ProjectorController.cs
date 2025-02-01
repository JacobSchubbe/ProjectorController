using Microsoft.AspNetCore.SignalR;
using ProjectController.Communication.Tcp;
using ProjectController.QueueManagement;
using static ProjectController.Controllers.Projector.ProjectorConstants;

namespace ProjectController.Controllers.Projector;

public class ProjectorController
{
    private readonly ILogger<ProjectorController> logger;
    private readonly IHubContext<GUIHub> hub;
    private readonly TcpCommunication tcpConnection;
    private readonly CommandRunner<ProjectorCommands> commandRunner;
    private bool startCommunicationSent = false;
    private bool isInitialVolumeQueryOnConnected = false;
    
    public ProjectorController(ILogger<ProjectorController> logger, 
        IHubContext<GUIHub> hub, 
        TcpCommunication tcpConnection,
        CommandRunner<ProjectorCommands> commandRunner)
    {
        this.logger = logger;
        this.hub = hub;
        this.tcpConnection = tcpConnection;
        this.commandRunner = commandRunner;
        tcpConnection.RegisterOnDisconnect(OnDisconnected);
        tcpConnection.RegisterOnConnect(OnConnected);
        commandRunner.PreCommandEvent += async cancellationToken =>
        {
            await this.tcpConnection.CheckConnection(cancellationToken);
        };
        Start().Wait();
    }

    private async Task Start()
    {
        await tcpConnection.Initialize(ProjectorHost, ProjectorPort);
        await commandRunner.Start(SendCommand);
        _ = UpdateVolumeRunner(CancellationToken.None);
    }

    private async Task OnConnected()
    {
        logger.LogInformation("Connected to projector.");
        await SendIsConnectedToProjector();
        await EnqueueCommand(ProjectorCommands.SystemControlStartCommunication);
        await EnqueueQuery(ProjectorCommands.SystemControlPowerQuery);
        isInitialVolumeQueryOnConnected = true;
        await EnqueueQuery(ProjectorCommands.SystemControlVolumeQuery);
    }
    
    private async Task OnDisconnected()
    {
        logger.LogInformation("Disconnected from projector.");
        startCommunicationSent = false;
        await SendIsConnectedToProjector();
    }

    public async Task SendIsConnectedToProjector()
    {
        logger.LogInformation($"Sending IsConnectedToProjector: {IsConnected}");
        await hub.Clients.All.SendAsync("IsConnectedToProjector", IsConnected);
    }

    private bool IsConnected => tcpConnection.IsConnected;

    public async Task EnqueueCommand(ProjectorCommands command, bool allowDuplicates = false)
    {
        await commandRunner.EnqueueCommand(new[] { command }, async (commandType, response) =>
        {
            await UpdateAllClients(commandType);
            await SendCommandResponseToClients(commandType, response);
        }, allowDuplicates);
    }
    
    public async Task EnqueueQuery(ProjectorCommands command, bool allowDuplicates = false)
    {
        await commandRunner.EnqueueCommand(new[] { command }, SendQueryResponseToClients, allowDuplicates);
    }
    
    private async Task<string> SendCommand(ProjectorCommands command)
    {
        var commandStr = $"{ProjectorCommandsDictionary[command]}\r";
        if (command is ProjectorCommands.SystemControlStartCommunication)
        {
            if (!startCommunicationSent)
            {
                commandStr = $"{ProjectorCommandsDictionary[command]}";
            }
            else
            {
                return string.Empty;
            }
        }
        
        return await tcpConnection.SendCommand(commandStr, CancellationToken.None);
    }
    
    private async Task SendCommandResponseToClients(ProjectorCommands commandType, string response)
    {
        if (commandType == ProjectorCommands.SystemControlStartCommunication)
        {
            if (string.IsNullOrEmpty(response))
            {
                logger.LogInformation($"Start communication failed.");
                startCommunicationSent = false;
                return;
            }
            
            logger.LogInformation($"Start communication acknowledged.");
            startCommunicationSent = true;
            return;
        }
        
        if (response == SuccessfulCommandResponse)
            response = $"Success! {response}";
        
        logger.LogInformation($"Sending command response: {response}");
        await hub.Clients.All.SendAsync("ReceiveMessage", new
        {
            message = $"System Control Command: {commandType} was successfully executed. Response: {response}"
        });
    }

    private async Task SendQueryResponseToClients(ProjectorCommands queryType, string rawResponse)
    {
        logger.LogTrace("Sending query response. QueryType: {$queryType} Raw response: {rawResponse}", queryType, rawResponse);
        switch (queryType)
        {
            case ProjectorCommands.SystemControlVolumeQuery:
                if (int.TryParse(rawResponse.Split('=')[1].TrimEnd(':', '\r'), out var rawVolume))
                {
                    await SetCurrentVolume(rawVolume);
                    await SendQueryResponse(queryType, targetVolume);
                }
                break;
            case ProjectorCommands.SystemControlPowerQuery:
                var status = StringToPowerStatus(rawResponse);
                logger.LogInformation($"Sending current status {status.ToString()} for query {queryType.ToString()}");
                await SendQueryResponse(queryType, status);
                break;
            default:
                rawResponse = rawResponse.Replace("=", " ").TrimEnd(':', '\r');
                ProjectorCommands? currentStatus = null;
                foreach (var kvp in ProjectorCommandsDictionary.Where(kvp => kvp.Value == rawResponse))
                {
                    currentStatus = kvp.Key;
                    break;
                }

                if (!currentStatus.HasValue && queryType != ProjectorCommands.SystemControlPowerQuery)
                {
                    logger.LogInformation($"No status matching current status: {rawResponse}");
                    return;
                }

                logger.LogInformation(
                    $"Sending current status {currentStatus.ToString()} for query {queryType.ToString()}");
                await SendQueryResponse(queryType, currentStatus);
                break;
        }
        
    }

    private async Task UpdateAllClients(ProjectorCommands commandType)
    {
        switch (commandType)
        {
            case ProjectorCommands.SystemControlSourceHDMI1:
            case ProjectorCommands.SystemControlSourceHDMI2:
            case ProjectorCommands.SystemControlSourceHDMI3:
            case ProjectorCommands.SystemControlSourceHDMILAN:
                logger.LogDebug($"Sending updated source to all clients: {commandType.ToString()}.");
                await SendQueryResponse(ProjectorCommands.SystemControlSourceQuery, commandType);
                break;
            case ProjectorCommands.SystemControlVolumeUp:
            case ProjectorCommands.SystemControlVolumeDown:
                logger.LogDebug($"Sending updated target volume to all clients: {commandType.ToString()}.");
                await SendQueryResponse(ProjectorCommands.SystemControlVolumeQuery, targetVolume);
                break;
        }
    }

    private async Task SendQueryResponse<T>(ProjectorCommands queryType, T status)
    {
        logger.LogInformation($"Sending query response: {status.ToString()} for query {queryType.ToString()}");
        await hub.Clients.All.SendAsync("ReceiveProjectorQueryResponse", new
        {
            queryType, currentStatus = status
        });
    }

    
    
    // TODO: Move to a separate class
    
    private int targetVolume = 0;
    private int currentVolume = 0;
    private const float MinVolume = 0;
    private const float MaxVolume = 248;
    private const float MaxDisplayVolume = 40;
    private TaskCompletionSource waitingForVolumeUpdate = new();
    private readonly SemaphoreSlim volumeUpdateSemaphore = new(1, 1);
    
    public async Task SetTargetVolume(int volume)
    {
        await volumeUpdateSemaphore.WaitAsync();
        targetVolume = volume;
        logger.LogDebug("Target volume set: {targetVolume}", targetVolume);
        volumeUpdateSemaphore.Release();
    }

    private async Task SetCurrentVolume(int volume)
    {
        await volumeUpdateSemaphore.WaitAsync();
        currentVolume = (int)Math.Round(volume / (MaxVolume - MinVolume) * MaxDisplayVolume + MinVolume, 0, MidpointRounding.AwayFromZero);
        if (isInitialVolumeQueryOnConnected)
        {
            targetVolume = currentVolume;
            logger.LogDebug("Initial Target Volume set: {targetVolume}", targetVolume);
            isInitialVolumeQueryOnConnected = false;
        }
        logger.LogDebug("Current volume set: {currentVolume}", currentVolume);
        waitingForVolumeUpdate.TrySetResult();
        volumeUpdateSemaphore.Release();
    }
    
    private async Task UpdateVolumeRunner(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await waitingForVolumeUpdate.Task;
            await volumeUpdateSemaphore.WaitAsync(token);
            try
            {
                var volumeDiff = targetVolume - currentVolume;
                if (volumeDiff == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), token);
                    continue;
                }
                
                var command = volumeDiff > 0 ? ProjectorCommands.SystemControlVolumeUp : ProjectorCommands.SystemControlVolumeDown;
                waitingForVolumeUpdate = new();
                await EnqueueCommand(command, true);
                await EnqueueQuery(ProjectorCommands.SystemControlVolumeQuery);
            }
            finally
            {
                volumeUpdateSemaphore.Release();
            }
        }
        logger.LogInformation("Update Volume Runner was cancelled.");
    }
}