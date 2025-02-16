using Microsoft.AspNetCore.SignalR;
using ProjectController.Controllers.ADB;
using ProjectController.Controllers.HdmiSwitch;
using ProjectController.Controllers.Projector;
using ProjectController.Controllers.TVControls;
using static ProjectController.Controllers.Projector.ProjectorConstants;

public class GUIHub : Hub
{
    private readonly ILogger<GUIHub> logger;
    private readonly ProjectorController projectorConnection;
    private readonly AndroidTVController adbConnection;
    private readonly HdmiSwitchController hdmiSwitchController;
    
    public GUIHub(
        ILogger<GUIHub> logger, 
        ProjectorController projectorConnection, 
        AndroidTVController adbConnection,
        HdmiSwitchController hdmiSwitchController)
    {
        this.logger = logger;
        this.projectorConnection = projectorConnection;
        this.adbConnection = adbConnection;
        this.hdmiSwitchController = hdmiSwitchController;
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
    
    public async Task ReceiveHdmiInput(Inputs input)
    {
        logger.LogTrace("Received hdmi input: {$input}", input);
        if (input == Inputs.SmartTV)
        {
            await ReceiveProjectorCommand(ProjectorCommandsEnum.SystemControlSourceHDMI3);
        }
        else
        {
            await ReceiveProjectorCommand(ProjectorCommandsEnum.SystemControlSourceHDMI1);
        }
        await hdmiSwitchController.SetInputHdmi(input);
    }
    
    public async Task ReceiveHdmiInputQuery()
    {
        logger.LogTrace("Received hdmi input query.");
        await hdmiSwitchController.ReadCurrentConfiguration();
    }
    
    public async Task ReceiveProjectorCommand(ProjectorCommandsEnum command)
    {
          logger.LogTrace($"Received projector command: {command.ToString()}");
        await projectorConnection.EnqueueCommand(command);
    }
    
    public async Task ReceiveProjectorQuery(ProjectorCommandsEnum command)
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
            await adbConnection.EnqueueCommand(command);
        }
        else
        {
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