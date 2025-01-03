using Microsoft.AspNetCore.SignalR;

public class GUIHub : Hub
{
    // Receive a message from a client
    public async Task SendMessage(string user, string message)
    {
        // Log or handle the received message
        Console.WriteLine($"Received message from {user}: {message}");

        // Send the message to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}