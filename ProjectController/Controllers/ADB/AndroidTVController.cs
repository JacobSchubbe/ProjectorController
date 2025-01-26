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
        await SendIsConnectedToProjector(true);
    }
    
    private async Task OnDisconnected(string updatedIp)
    {
        if (ip != updatedIp)
            return;
        logger.LogInformation("Disconnected from AndroidTV.");
        await SendIsConnectedToProjector(false);
    }
    
    public bool IsConnected => adbController.IsConnected();

    public async Task SendIsConnectedToProjector(bool isConnected)
    {
        logger.LogInformation($"Sending IsConnectedToAndroidTVQuery: {isConnected}");
        await hub.Clients.All.SendAsync("IsConnectedToAndroidTVQuery", isConnected);
    }

    public async Task EnqueueCommand(KeyCodes command)
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
        if (!adbController.IsConnected())
        {
            var timeout = 3;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            try
            {
                await adbController.Connect(cts.Token);
            }
            catch (OperationCanceledException)
            {
                return $"Failed to connect to device after {timeout} seconds.";
            }
            
            await SendIsConnectedToProjector(IsConnected);
        }

        try
        {
            await adbController.KeyCommands[command]();
            return "Success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}