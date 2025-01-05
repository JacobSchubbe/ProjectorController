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
    
    // Receive a message from a client
    public async Task ReceiveSystemCommand(SystemControl command)
    {
        // Log or handle the received message
        Console.WriteLine($"Received command: {command.ToString()}");

        await tcpConnection.QueueCommand([SystemControlDictionary[command]]);
        
        // Send the message to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", $"{command.ToString()} was successfully executed.");
    }
}