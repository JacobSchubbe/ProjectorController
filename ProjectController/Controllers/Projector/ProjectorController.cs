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
    private int targetVolume = 0;
    private int currentVolume = 0;
    private readonly SemaphoreSlim volumeUpdateSemaphore = new(1, 1);
    
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
        // _ = UpdateVolumeRunner(CancellationToken.None);
    }

    private async Task OnConnected()
    {
        logger.LogInformation("Connected to projector.");
        await SendIsConnectedToProjector();
        await EnqueueCommand(ProjectorCommands.SystemControlStartCommunication);
        await EnqueueQuery(ProjectorCommands.SystemControlPowerQuery);
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

    public async Task EnqueueCommand(ProjectorCommands command, bool duplicatesAllowed = false)
    {
        await commandRunner.EnqueueCommand(new[] { command }, async (commandType, response) =>
        {
            await UpdateAllClients(commandType);
            await SendCommandResponseToClients(commandType, response);
        }, duplicatesAllowed);
    }
    
    public async Task EnqueueQuery(ProjectorCommands command)
    {
        
        await commandRunner.EnqueueCommand(new[] { command }, SendQueryResponseToClients);
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
        logger.LogInformation($"Sending query response. Raw response: {rawResponse}");
        var status = StringToPowerStatus(rawResponse);
        if (status == null)
        {
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
        }
        else
        {
            logger.LogInformation($"Sending current status {status.ToString()} for query {queryType.ToString()}");
            await SendQueryResponse(queryType, status);
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
        }
    }

    private async Task SendQueryResponse<T>(ProjectorCommands queryType, T status)
    {
        await hub.Clients.All.SendAsync("ReceiveProjectorQueryResponse", new
        {
            queryType, currentStatus = status
        });
    }

    public async Task SetVolume(int volume)
    {
        await volumeUpdateSemaphore.WaitAsync();
        targetVolume = volume;
        volumeUpdateSemaphore.Release();
    }
    
    private async Task UpdateVolumeRunner(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await volumeUpdateSemaphore.WaitAsync(token);
            try
            {
                var volumeDiff = targetVolume - currentVolume;
                if (volumeDiff == 0)
                    continue;
                
                var command = volumeDiff > 0 ? ProjectorCommands.SystemControlVolumeUp : ProjectorCommands.SystemControlVolumeDown;
                for (var i = 0; i < Math.Abs(volumeDiff); i++)
                {
                    await EnqueueCommand(command, true);
                }
                currentVolume = targetVolume;
                await EnqueueQuery(ProjectorCommands.SystemControlVolumeQuery);
                await Task.Delay(20, token);
            }
            finally
            {
                volumeUpdateSemaphore.Release();
            }
        }
        logger.LogInformation("Update Volume Runner was cancelled.");
    }
}