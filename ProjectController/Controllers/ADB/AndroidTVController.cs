using Microsoft.AspNetCore.SignalR;
using ProjectController.Controllers.Projector;
using ProjectController.QueueManagement;

namespace ProjectController.Controllers.ADB;

public class AndroidTVController
{
    private readonly ILogger<ProjectorController> logger;
    private readonly IHubContext<GUIHub> hub;
    private readonly AdbController adbController;
    private readonly CommandRunner<KeyCodes> commandRunner;
    private string ip => adbController.Ip;
    public AndroidTVController(ILogger<ProjectorController> logger, IHubContext<GUIHub> hub, AdbController adbController, CommandRunner<KeyCodes> commandRunner)
    {
        this.logger = logger;
        this.hub = hub;
        this.adbController = adbController;
        this.commandRunner = commandRunner;
        _ = Start();
    }

    private async Task Start()
    {
        adbController.AdbClient.RegisterOnDisconnect(OnDisconnected);
        adbController.AdbClient.RegisterOnConnect(OnConnected);
        await commandRunner.Start(SendCommand);
        await adbController.Connect(CancellationToken.None);
    }
    
    private async Task OnConnected(string updatedIp)
    {
        if (ip != updatedIp)
            return;
        logger.LogInformation("Connected to AndroidTV.");
        await SendIsConnectedToAndroidTV(true);
    }
    
    private async Task OnDisconnected(string updatedIp)
    {
        if (ip != updatedIp)
            return;
        logger.LogInformation("Disconnected from AndroidTV. Trying to reconnect.");
        await SendIsConnectedToAndroidTV(false);
        _ = TryToReconnect();
    }

    private async Task TryToReconnect()
    {
        try
        {
            await adbController.Connect(CancellationToken.None);
            await SendIsConnectedToAndroidTV(IsConnected);
        }
        catch (Exception)
        {
            logger.LogWarning("Failed to connect to device at ip {ip}", ip);
        }
    }
    
    public bool IsConnected => adbController.IsConnected();

    public async Task SendIsConnectedToAndroidTV(bool isConnected)
    {
        logger.LogInformation($"Sending IsConnectedToAndroidTVQuery: {isConnected}");
        await hub.Clients.All.SendAsync("IsConnectedToAndroidTVQuery", isConnected);
    }

    public async Task EnqueueCommand(KeyCodes command)
    {
        await commandRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
    }
    
    public async Task EnqueueLongPressCommand(KeyCodes command)
    {
        await commandRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
    }
    
    public Task EnqueueOpenAppCommand(KeyCodes command)
    {
        var app = (command) switch
        {
            KeyCodes.Netflix => AndroidTVApps.Netflix,
            KeyCodes.Youtube => AndroidTVApps.YouTube,
            KeyCodes.AmazonPrime => AndroidTVApps.AmazonPrime, 
            KeyCodes.DisneyPlus => AndroidTVApps.DisneyPlus, 
            KeyCodes.Crunchyroll => AndroidTVApps.Crunchyroll,
            KeyCodes.Surfshark => AndroidTVApps.Surfshark,
            KeyCodes.Spotify => AndroidTVApps.Spotify,
            _ => throw new NotImplementedException()
        };
        
        adbController.OpenApp(app);
        // await commandRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
        return Task.CompletedTask;
    }
    
    public async Task EnqueueQuery(KeyCodes command)
    {
        await commandRunner.EnqueueCommand(new[] { command }, SendQueryResponseToClients);
    }

    private Task SendCommandResponseToClients(KeyCodes commandType, string response)
    {
        // logger.LogInformation($"Sending command response: {response}");
        // await hub.Clients.All.SendAsync("ReceiveMessage", new
        // {
        //     message = $"System Control Command: {commandType} was successfully executed. Response: {response}"
        // });
        return Task.CompletedTask;
    }

    private Task SendQueryResponseToClients(KeyCodes queryType, string rawResponse)
    {
        // logger.LogInformation($"Sending query response. Raw response: {rawResponse}");
        // await hub.Clients.All.SendAsync("ReceiveMessage", new
        // {
        //     message = $"System Control Query: {queryType} was successfully executed. Response: {rawResponse}"
        // });
        return Task.CompletedTask;
    }

    private async Task<string> SendCommand(KeyCodes command)
    {
        try
        {
            await adbController.KeyCommands[command]();
            return "Success";
        }
        catch (Exception ex)
        {
            logger.LogDebug("Error while sending command: {error}", ex.Message);
            return ex.Message;
        }
    }
}