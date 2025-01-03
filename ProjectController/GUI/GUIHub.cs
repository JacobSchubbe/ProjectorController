using Microsoft.AspNetCore.SignalR;

namespace ProjectController.GUI;

public class GUIHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}