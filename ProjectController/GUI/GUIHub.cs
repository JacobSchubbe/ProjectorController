using Microsoft.AspNetCore.SignalR;
using ProjectController.TCPCommunication;
using static ProjectController.TCPCommunication.TCPConsts;

public class GUIHub : Hub
{
    private readonly TcpConnection connection;
    
    public GUIHub(TcpConnection tcpConnection)
    {
        this.connection = tcpConnection;
    }
    
    // Receive a message from a client
    public async Task ReceiveSystemCommand(SystemControl command)
    {
        // Log or handle the received message
        Console.WriteLine($"Received command: {command.ToString()}");

        connection.RunCommand();
        
        // Send the message to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", $"{command.ToString()} was successfully executed.");
    }
}