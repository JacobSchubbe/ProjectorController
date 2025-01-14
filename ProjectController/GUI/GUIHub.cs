using Microsoft.AspNetCore.SignalR;
using ProjectController.ADB;
using ProjectController.TCPCommunication;
using static ProjectController.TCPCommunication.TCPConsts;

public class GUIHub : Hub
{
    private readonly ILogger<GUIHub> logger;
    private readonly TcpConnection tcpConnection;
    
    public GUIHub(ILogger<GUIHub> logger, TcpConnection tcpConnection, AndroidTVController androidTVController)
    {
        this.logger = logger;
        this.tcpConnection = tcpConnection;
        this.tcpConnection.RegisterOnDisconnect(SendIsConnectedToProjector);
    }
    
    private async Task SendIsConnectedToProjector(bool isConnected)
    {
        logger.LogInformation($"Sending IsConnectedToProjector: {isConnected}");
        await Clients.All.SendAsync("IsConnectedToProjector", isConnected);
    }
    
    public async Task IsConnectedToProjectorQuery()
    {
        logger.LogInformation("Received: IsConnectedToProjectorQuery");
        await SendIsConnectedToProjector(tcpConnection.IsConnected);
    }
    
    public async Task ReceiveProjectorCommand(ProjectorCommands command)
    {
        logger.LogInformation($"Received command: {command.ToString()}");
        await tcpConnection.QueueCommand(new []{command}, SendCommandResponseToClients);
    }
    
    public async Task ReceiveProjectorQuery(ProjectorCommands command)
    {
        logger.LogInformation($"Received query: {command.ToString()}");
        await tcpConnection.QueueCommand(new []{command}, SendQueryResponseToClients);
    }
    
    private async Task SendCommandResponseToClients(ProjectorCommands commandType, string response)
    {
        if (response == SuccessfulCommandResponse)
            response = $"Success! {response}";
        
        logger.LogInformation($"Sending command response: {response}");
        await Clients.All.SendAsync("ReceiveMessage", new
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

            logger.LogInformation($"Sending current status {currentStatus.ToString()} for query {queryType.ToString()}");
            await Clients.All.SendAsync("ReceiveProjectorQueryResponse", new
            {
                queryType, currentStatus = currentStatus
            });
        }
        else
        {
            logger.LogInformation($"Sending current status {status.ToString()} for query {queryType.ToString()}");
            await Clients.All.SendAsync("ReceiveProjectorQueryResponse", new
            {
                queryType, currentStatus = status
            });
        }
    }
}