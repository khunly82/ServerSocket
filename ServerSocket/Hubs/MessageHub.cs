using Microsoft.AspNetCore.SignalR;

namespace ServerSocket.Hubs
{
    public class MessageHub: Hub
    {
        private static readonly List<Game> Games = [];

        public async Task Join(string groupName)
        {
            Game? g = Games.FirstOrDefault(g => g.Name == groupName);
            if(g == null)
            {
                await Clients.Caller.SendAsync("error", "This game does not exists");
            }
            else if (g.OpponentId != null) 
            {
                await Clients.Caller.SendAsync("error", "This game is not available");
            }
            else
            {
                g.OpponentId = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                await Clients.All.SendAsync("availableGames", Games.Where(g => g.OpponentId == null).Select(g => g.Name));
            }
        }

        public async Task Create(string groupName)
        {
            if(Games.Any(g => g.Name == groupName))
            {
                await Clients.Caller.SendAsync("error", "This name is invalid");
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                Games.Add(new Game(groupName, Context.ConnectionId));
                await Clients.All.SendAsync("availableGames", Games.Where(g => g.OpponentId == null).Select(g => g.Name));
            }
        }

        public async Task SendMessage(Message message)
        {
            await Clients.OthersInGroup(message.Group).SendAsync("message", message.Value);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("availableGames", Games.Where(g => g.OpponentId == null).Select(g => g.Name));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Game? g = Games.FirstOrDefault(g => g.OwnerId == Context.ConnectionId || g.OpponentId == Context.ConnectionId);
            if(g != null)
            {
                await Clients.Group(g.Name).SendAsync("error", "Your opponent has left");
                Games.Remove(g);
            }
            await Clients.All.SendAsync("availableGames", Games.Where(g => g.OpponentId == null).Select(g => g.Name));
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

    public class Message {
        public string Group { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
