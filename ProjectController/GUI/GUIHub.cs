using Microsoft.AspNetCore.SignalR;
using ProjectController.ADB;
using ProjectController.Projector;
using ProjectController.TVControls;
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
          logger.LogTrace("Received: IsConnectedToProjectorQuery");
        await projectorConnection.SendIsConnectedToProjector();
    }
    
    public async Task IsConnectedToAndroidTVQuery()
    {
          logger.LogTrace("Received: IsConnectedToAndroidTVQuery");
        await adbConnection.SendIsConnectedToProjector(adbConnection.IsConnected);
    }
    
    public async Task ReceiveProjectorCommand(ProjectorCommands command)
    {
          logger.LogTrace($"Received projector command: {command.ToString()}");
        await projectorConnection.EnqueueCommand(command);
    }
    
    public async Task ReceiveProjectorQuery(ProjectorCommands command)
    {
          logger.LogTrace($"Received projector query: {command.ToString()}");
        await projectorConnection.EnqueueQuery(command);
    }
    
    public Task ReceiveTVCommand(IRCommands command)
    {
          logger.LogTrace($"Received TV command: {command.ToString()}");
        IRCommandManager.SendIRCommand(command);
        return Task.CompletedTask;
    }
    
    public async Task ReceiveAndroidCommand(KeyCodes command)
    {
          logger.LogTrace($"Received android command: {command.ToString()}");
        await adbConnection.EnqueueCommand(command);
    }
    
    public async Task ReceiveAndroidOpenAppCommand(KeyCodes command)
    {
          logger.LogTrace($"Received open app command: {command.ToString()}");
        await adbConnection.EnqueueOpenAppCommand(command);
    }
    
    public async Task ReceiveAndroidQuery(KeyCodes command)
    {
          logger.LogTrace($"Received android query: {command.ToString()}");
        await adbConnection.EnqueueQuery(command);
    }
}