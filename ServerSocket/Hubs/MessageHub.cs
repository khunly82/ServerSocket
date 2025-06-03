using Microsoft.AspNetCore.SignalR;

namespace ServerSocket.Hubs
{
    public class MessageHub: Hub
    { 
        public async Task Connect(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessage(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync(message);
        }
    }
}
