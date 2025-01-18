using Microsoft.AspNetCore.SignalR;
using ProjectController.Projector;
using ProjectController.QueueManagement;

namespace ProjectController.ADB;

public class AdbConnection
{
    private readonly ILogger<ProjectorConnection> logger;
    private readonly IHubContext<GUIHub> hub;
    private readonly AndroidTVController androidTvController;
    private readonly TaskRunner<KeyCodes> taskRunner;
    private string ip => androidTvController.Ip;
    public AdbConnection(ILogger<ProjectorConnection> logger, IHubContext<GUIHub> hub, AndroidTVController androidTvController, TaskRunner<KeyCodes> taskRunner)
    {
        this.logger = logger;
        this.hub = hub;
        this.androidTvController = androidTvController;
        this.taskRunner = taskRunner;
        _ = Start();
    }

    private async Task Start()
    {
        androidTvController.AdbClient.RegisterOnDisconnect(OnDisconnected);
        androidTvController.AdbClient.RegisterOnConnect(OnConnected);
        await taskRunner.Start(SendCommand);
        await androidTvController.Connect(CancellationToken.None);
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
    
    public bool IsConnected => androidTvController.IsConnected();

    public async Task SendIsConnectedToProjector(bool isConnected)
    {
        logger.LogInformation($"Sending IsConnectedToAndroidTVQuery: {isConnected}");
        await hub.Clients.All.SendAsync("IsConnectedToAndroidTVQuery", isConnected);
    }

    public async Task EnqueueCommand(KeyCodes command)
    {
        await taskRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
    }
    
    public Task EnqueueOpenAppCommand(KeyCodes command)
    {
        var app = (command) switch
        {
            KeyCodes.Netflix => AndroidTVApps.Netflix,
            KeyCodes.Youtube => AndroidTVApps.YouTube,
            KeyCodes.AmazonPrime => AndroidTVApps.AmazonPrime, 
            _ => throw new NotImplementedException()
        };
        
        androidTvController.OpenApp(app);
        // await taskRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
        return Task.CompletedTask;
    }
    
    public async Task EnqueueQuery(KeyCodes command)
    {
        await taskRunner.EnqueueCommand(new[] { command }, SendQueryResponseToClients);
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
        if (!androidTvController.IsConnected())
        {
            var timeout = 3;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            try
            {
                await androidTvController.Connect(cts.Token);
            }
            catch (OperationCanceledException)
            {
                return $"Failed to connect to device after {timeout} seconds.";
            }
            
            await SendIsConnectedToProjector(IsConnected);
        }

        try
        {
            await androidTvController.KeyCommands[command]();
            return "Success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}