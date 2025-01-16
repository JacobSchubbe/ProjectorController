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

    public AdbConnection(ILogger<ProjectorConnection> logger, IHubContext<GUIHub> hub, AndroidTVController androidTvController, TaskRunner<KeyCodes> taskRunner)
    {
        this.logger = logger;
        this.hub = hub;
        this.androidTvController = androidTvController;
        this.taskRunner = taskRunner;
        Start().Wait();
    }

    private async Task Start()
    {
        await taskRunner.Start(SendCommand);
        // androidTvController.Connect();
    }

    public async Task EnqueueCommand(KeyCodes command)
    {
        await taskRunner.EnqueueCommand(new[] { command }, SendCommandResponseToClients);
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
            if (!androidTvController.Connect())
                return "Failed to connect to device.";
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