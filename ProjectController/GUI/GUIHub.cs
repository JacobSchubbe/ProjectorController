using Microsoft.AspNetCore.SignalR;
using ProjectController.Controllers.ADB;
using ProjectController.Controllers.Projector;
using ProjectController.Controllers.TVControls;
using static ProjectController.Controllers.Projector.ProjectorConstants;

public class GUIHub : Hub
{
    private readonly ILogger<GUIHub> logger;
    private readonly ProjectorController projectorConnection;
    private readonly AndroidTVController adbConnection;
    
    public GUIHub(ILogger<GUIHub> logger, ProjectorController projectorConnection, AndroidTVController adbConnection)
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
        await adbConnection.SendIsConnectedToAndroidTV(adbConnection.IsConnected);
    }

    public async Task ReceiveProjectorVolumeSet(int volume)
    {
        await projectorConnection.SetTargetVolume(volume);
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
    
    public async Task ReceiveAndroidCommand(KeyCodes command, bool isLongPress)
    {
        logger.LogTrace($"Received android command: {command.ToString()} with LongPress: {isLongPress}");
    
        if (isLongPress)
        {
            // Handle long press logic
            await adbConnection.EnqueueLongPressCommand(command);
        }
        else
        {
            // Handle regular short press
            await adbConnection.EnqueueCommand(command);
        }
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