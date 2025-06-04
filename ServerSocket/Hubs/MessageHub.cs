using Microsoft.AspNetCore.SignalR;

namespace ServerSocket.Hubs
{
    public class MessageHub: Hub
    {
        private static readonly List<Game> Games = [];

        public async Task Join(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Game? g = Games.FirstOrDefault(g => g.Name == groupName);
            if(g == null)
            {
                await Clients.Caller.SendAsync("Error", "This game does not exists");
            }
            else if (g.OpponentId != null) 
            {
                await Clients.Caller.SendAsync("Error", "This game is not available");
            }
        }

        public async Task Create(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Games.Add(new Game(groupName, Context.ConnectionId));
        }

        public async Task SendMessage(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync(message);
        }

        public override async Task OnConnectedAsync()
        {
            Game? g = Games.FirstOrDefault(g => g.OwnerId == Context.ConnectionId || g.OpponentId == Context.ConnectionId);
            if(g != null)
            {
                await Clients.Group(g.Name).SendAsync("Error", "Your opponent has left");
                Games.Remove(g);
            }
        }
    }

    public class Game
    {
        public Game(string name, string ownerId)
        {
            Name = name;
            OwnerId = ownerId;
        }

        public string Name { get; set; }
        public string OwnerId { get; set; }
        public string? OpponentId { get; set; }
    }
}
