using System.Text;
using Microsoft.AspNetCore.SignalR;
using ProjectController.TCPCommunication;
using static ProjectController.TCPCommunication.TCPConsts;

public class GUIHub : Hub
{
    private readonly TcpConnection tcpConnection;
    
    public GUIHub(TcpConnection tcpConnection)
    {
        this.tcpConnection = tcpConnection;
    }
    
    public async Task ReceiveProjectorCommand(ProjectorCommands command)
    {
        // Log or handle the received message
        Console.WriteLine($"Received command: {command.ToString()}");

        await tcpConnection.QueueCommand(new []{command}, SendCommandResponseToClients);
    }
    
    public async Task ReceiveProjectorQuery(ProjectorCommands command)
    {
        // Log or handle the received message
        Console.WriteLine($"Received query: {command.ToString()}");

        await tcpConnection.QueueCommand(new []{command}, SendQueryResponseToClients);
    }
    
    private async Task SendCommandResponseToClients(ProjectorCommands commandType, string response)
    {
        if (response == SuccessfulCommandResponse)
            response = $"Success! {response}";
        
        // Send the message to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", new
        {
            message = $"System Control Command: {commandType} was successfully executed. Response: {response}"
        });
    }

    private async Task SendQueryResponseToClients(ProjectorCommands queryType, string response)
    {
        response = response.Replace("=", " ").TrimEnd(':', '\r');
        ProjectorCommands? currentStatus = null;
        foreach (var kvp in ProjectorCommandsDictionary)
        {
            if (kvp.Value != response) continue;
            currentStatus = kvp.Key;
            break;
        }

        if (currentStatus == null)
        {       
            Console.WriteLine($"No status matching current status: {response}");
            return;
        }

        Console.WriteLine($"Sending current status {currentStatus.ToString()} for query {queryType.ToString()}");
        await Clients.All.SendAsync("ReceiveProjectorQueryResponse", new
        {
            queryType, currentStatus
        });
    }
}