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
        adbController.AdbClient.RegisterOnDisconnect(OnDisconnected);
        adbController.AdbClient.RegisterOnVpnConnect(() => SendQueryResponseToClients(KeyCodes.VpnStatusQuery, true.ToString()));
        adbController.AdbClient.RegisterOnVpnDisconnect(() => SendQueryResponseToClients(KeyCodes.VpnStatusQuery, false.ToString()));
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
    public bool IsVpnConnected => adbController.IsVpnConnected();

    public async Task SendIsConnectedToAndroidTV(bool isConnected)
    {
        logger.LogInformation($"Sending IsConnectedToAndroidTVQuery: {isConnected}");
        await hub.Clients.All.SendAsync("IsConnectedToAndroidTVQuery", isConnected);
    }

    public async Task EnqueueCommand(KeyCodes command)
    {
        await EnqueueCommand(command, false);
    }
    
    public async Task EnqueueLongPressCommand(KeyCodes command)
    {
        await EnqueueCommand(command, true);
    }

    private async Task EnqueueCommand(KeyCodes command, bool isLongPress)
    {
        await commandRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients, allowDuplicates:true);
    }
    
    public async Task EnqueueOpenAppCommand(KeyCodes command)
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
        
        await adbController.OpenApp(app);
        // await commandRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
    }
    
    public async Task EnqueueQuery(KeyCodes command)
    {
        await commandRunner.EnqueueCommand(new[] { command }, SendQueryResponseToClients);
    }

    private async Task SendCommandResponseToClients(KeyCodes commandType, string response)
    {
        logger.LogInformation($"Sending command response: {response}");
        if (response == AdbConstants.AdbSuccess)
        {
            switch (commandType)
            {
                case KeyCodes.VpnOff:
                case KeyCodes.VpnOn:
                    await hub.Clients.All.SendAsync("ReceiveAndroidTVQueryResponse", new
                    {
                        KeyCodes.VpnStatusQuery, currentStatus = commandType
                    });
                    break;
            }
        }
        else
        {
            await hub.Clients.All.SendAsync("ReceiveMessage", new
            {
                message = $"System Control Command: {commandType} was successfully executed. Response: {response}"
            });
        }
    }

    private async Task SendQueryResponseToClients(KeyCodes queryType, string rawResponse)
    {
        logger.LogInformation($"Sending query response. Raw response: {rawResponse}");
        switch (queryType)
        {
            case KeyCodes.VpnStatusQuery:
            {
                var isConnected = bool.Parse(rawResponse);
                var response = isConnected ? KeyCodes.VpnOn : KeyCodes.VpnOff;
                await hub.Clients.All.SendAsync("ReceiveAndroidTVQueryResponse", new
                {
                    queryType, currentStatus = response
                });
                break;
            }
        }
    }

    private async Task<string> SendCommand(KeyCodes command)
    {
        try
        {
            return await adbController.KeyCommands[command]();
        }
        catch (Exception ex)
        {
            logger.LogDebug("Error while sending command: {error}", ex.Message);
            return ex.Message;
        }
    }
}