using Microsoft.AspNetCore.SignalR;
using ProjectController.ADB;
using ProjectController.Projector;
using static ProjectController.Projector.ProjectorConstants;

public class GUIHub : Hub
{
    private readonly ILogger<GUIHub> logger;
    private readonly ProjectorConnection projectorConnection;
    private readonly AdbConnection adbConnection;
    
    public GUIHub(ILogger<GUIHub> logger, ProjectorConnection projectorConnection, AdbConnection adbConnection)
    {
        this.logger = logger;
        this.projectorConnection = projectorConnection;
        this.adbConnection = adbConnection;
    }

    public async Task Ping()
    {
        logger.LogTrace("Received heartbeat (Ping) from client.");
        await Clients.All.SendAsync("ReceivedPing");
    }
    
    public async Task IsConnectedToProjectorQuery()
    {
        logger.LogInformation("Received: IsConnectedToProjectorQuery");
        await projectorConnection.SendIsConnectedToProjector(projectorConnection.IsConnected);
    }
    
    public async Task IsConnectedToAndroidTVQuery()
    {
        logger.LogInformation("Received: IsConnectedToAndroidTVQuery");
        await adbConnection.SendIsConnectedToProjector(adbConnection.IsConnected);
    }
    
    public async Task ReceiveProjectorCommand(ProjectorCommands command)
    {
        logger.LogInformation($"Received command: {command.ToString()}");
        await projectorConnection.EnqueueCommand(command);
    }
    
    public async Task ReceiveProjectorQuery(ProjectorCommands command)
    {
        logger.LogInformation($"Received query: {command.ToString()}");
        await projectorConnection.EnqueueQuery(command);
    }
    
    public async Task ReceiveAndroidCommand(KeyCodes command)
    {
        logger.LogInformation($"Received command: {command.ToString()}");
        await adbConnection.EnqueueCommand(command);
    }
    
    public async Task ReceiveAndroidOpenAppCommand(KeyCodes command)
    {
        logger.LogInformation($"Received open app command: {command.ToString()}");
        await adbConnection.EnqueueOpenAppCommand(command);
    }
    
    public async Task ReceiveAndroidQuery(KeyCodes command)
    {
        logger.LogInformation($"Received query: {command.ToString()}");
        await adbConnection.EnqueueQuery(command);
    }
}