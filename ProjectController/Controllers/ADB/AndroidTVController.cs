using Microsoft.AspNetCore.SignalR;
using ProjectController.QueueManagement;

namespace ProjectController.Controllers.ADB;

public class AndroidTVController
{
    private readonly ILogger<AndroidTVController> logger;
    private readonly IHubContext<GUIHub> hub;
    private readonly AdbController adbController;
    private readonly CommandRunner<AndroidTVCommand, KeyCodes> commandRunner;
    private string ip => adbController.Ip;
    public AndroidTVController(ILogger<AndroidTVController> logger, IHubContext<GUIHub> hub, AdbController adbController, CommandRunner<AndroidTVCommand, KeyCodes> commandRunner)
    {
        this.logger = logger;
        this.hub = hub;
        this.adbController = adbController;
        this.commandRunner = commandRunner;
        adbController.AdbClient.RegisterOnDisconnect(OnDisconnected);
        adbController.AdbClient.RegisterOnVpnConnect(() => SendQueryResponseToClients(new AndroidTVCommand(KeyCodes.VpnStatusQuery, _ => Task.FromResult(string.Empty), SendQueryResponseToClients, false), true.ToString()));
        adbController.AdbClient.RegisterOnVpnDisconnect(() => SendQueryResponseToClients(new AndroidTVCommand(KeyCodes.VpnStatusQuery, _ => Task.FromResult(string.Empty), SendQueryResponseToClients, false), false.ToString()));
        _ = Start();
    }

    private async Task Start()
    {
        adbController.AdbClient.RegisterOnDisconnect(OnDisconnected);
        adbController.AdbClient.RegisterOnConnect(OnConnected);
        await commandRunner.Start();
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

    public async Task EnqueueCommand(KeyCodes commandType)
    {
        await EnqueueCommand(new AndroidTVCommand(commandType, command => SendCommand((AndroidTVCommand)command), SendCommandResponseToClients, false));
    }
    
    public async Task EnqueueLongPressCommand(KeyCodes commandType)
    {
        await EnqueueCommand(new AndroidTVCommand(commandType, command => SendCommand((AndroidTVCommand)command), SendCommandResponseToClients, true));
    }

    private async Task EnqueueCommand(AndroidTVCommand command)
    {
        await commandRunner.EnqueueCommand(new[] { command }, allowDuplicates:true);
    }
    
    public async Task EnqueueQuery(KeyCodes commandType)
    {
        await EnqueueCommand(new AndroidTVCommand(commandType, command => SendCommand((AndroidTVCommand)command), SendQueryResponseToClients, false));
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
    
    private async Task SendCommandResponseToClients(ICommand<KeyCodes> command, string response)
    {
        logger.LogInformation($"Sending command response: {response}");
        if (response == AdbConstants.AdbSuccess)
        {
            switch (command.CommandType)
            {
                case KeyCodes.VpnOff:
                case KeyCodes.VpnOn:
                    await hub.Clients.All.SendAsync("ReceiveAndroidTVQueryResponse", new
                    {
                        KeyCodes.VpnStatusQuery, currentStatus = command.CommandType
                    });
                    break;
            }
        }
        else
        {
            await hub.Clients.All.SendAsync("ReceiveMessage", new
            {
                message = $"System Control Command: {command.CommandType} was successfully executed. Response: {response}"
            });
        }
    }

    private async Task SendQueryResponseToClients(ICommand<KeyCodes> command, string rawResponse)
    {
        var queryType = command.CommandType;
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

    private async Task<string> SendCommand(AndroidTVCommand command)
    {
        try
        {
            logger.LogInformation($"Sending command: {command.CommandType}, with long-press: {command.IsLongPress}.");
            return await adbController.KeyCommands[command.CommandType](command.IsLongPress);
        }
        catch (Exception ex)
        {
            logger.LogDebug("Error while sending command: {error}", ex.Message);
            return ex.Message;
        }
    }
}