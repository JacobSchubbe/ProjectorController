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
    
    public async Task ReceiveSystemCommand(SystemControl command)
    {
        // Log or handle the received message
        Console.WriteLine($"Received command: {command.ToString()}");

        await tcpConnection.QueueCommand(new []{command}, SendCommandResponseToClients);
    }
    
    public async Task ReceiveKeyCommand(KeyControl command)
    {
        // Log or handle the received message
        Console.WriteLine($"Received command: {command.ToString()}");

        await tcpConnection.QueueCommand(new []{command}, SendCommandResponseToClients);
    }
    
    public async Task ReceiveSystemQuery(SystemControl command)
    {
        // Log or handle the received message
        Console.WriteLine($"Received query: {command.ToString()}");

        await tcpConnection.QueueCommand(new []{command}, SendQueryResponseToClients);
    }

    private async Task SendCommandResponseToClients(KeyControl commandType, string response)
    {
        if (response == SuccessfulCommandResponse)
            response = "Success!";
        
        // Send the message to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", new
        {
            message = $"Key Command: {commandType} was successfully executed. Response: {response}"
        });
    }
    
    private async Task SendCommandResponseToClients(SystemControl commandType, string response)
    {
        if (response == SuccessfulCommandResponse)
            response = "Success!";
        
        // Send the message to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", new
        {
            message = $"System Control Command: {commandType} was successfully executed. Response: {response}"
        });
    }

    private async Task SendQueryResponseToClients(SystemControl queryType, string response)
    {
        response = response.Replace("=", " ").TrimEnd(':', '\r');
        SystemControl? currentStatus = null;
        foreach (var kvp in SystemControlCommands)
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
        await Clients.All.SendAsync("ReceiveQueryResponse", new
        {
            queryType, currentStatus
        });
    }
}