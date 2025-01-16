using Microsoft.AspNetCore.SignalR;
using ProjectController.QueueManagement;
using ProjectController.TCPCommunication;
using static ProjectController.Projector.ProjectorConstants;

namespace ProjectController.Projector;

public class ProjectorConnection
{
    private readonly ILogger<ProjectorConnection> logger;
    private readonly IHubContext<GUIHub> hub;
    private readonly TcpConnection tcpConnection;
    private readonly TaskRunner<ProjectorCommands> taskRunner;
    
    public ProjectorConnection(ILogger<ProjectorConnection> logger, 
        IHubContext<GUIHub> hub, 
        TcpConnection tcpConnection,
        TaskRunner<ProjectorCommands> queueRunner)
    {
        this.logger = logger;
        this.hub = hub;
        this.tcpConnection = tcpConnection;
        this.taskRunner = queueRunner;
        tcpConnection.RegisterOnDisconnect(SendIsConnectedToProjector);
        tcpConnection.RegisterOnConnect(OnConnected);
        taskRunner.PreCommandEvent += async cancellationToken => await this.tcpConnection.CheckConnection(cancellationToken);
        Start().Wait();
    }

    private async Task Start()
    {
        await tcpConnection.Start(ProjectorHost, ProjectorPort);
        await taskRunner.Start(SendCommand);
    }

    private async Task OnConnected()
    {
        await SendCommand(ProjectorCommands.SystemControlStartCommunication);
    }

    public async Task SendIsConnectedToProjector(bool isConnected)
    {
        logger.LogInformation($"Sending IsConnectedToProjector: {isConnected}");
        await hub.Clients.All.SendAsync("IsConnectedToProjector", isConnected);
    }
    
    public bool IsConnected => tcpConnection.IsConnected;

    public async Task EnqueueCommand(ProjectorCommands command)
    {
        await taskRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
    }
    
    public async Task EnqueueQuery(ProjectorCommands command)
    {
        await taskRunner.EnqueueCommand(new[] { command }, SendQueryResponseToClients);
    }
    
    private Task<string> SendCommand(ProjectorCommands command)
    {
        var commandStr = command != ProjectorCommands.SystemControlStartCommunication ? 
            $"{ProjectorCommandsDictionary[command]}\r" : $"{ProjectorCommandsDictionary[command]}";
        return Task.FromResult(tcpConnection.SendCommand(commandStr));
    }
    
    private async Task SendCommandResponseToClients(ProjectorCommands commandType, string response)
    {
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
            await hub.Clients.All.SendAsync("ReceiveProjectorQueryResponse", new
            {
                queryType, currentStatus
            });
        }
        else
        {
            logger.LogInformation($"Sending current status {status.ToString()} for query {queryType.ToString()}");
            await hub.Clients.All.SendAsync("ReceiveProjectorQueryResponse", new
            {
                queryType, currentStatus = status
            });
        }
    }
}