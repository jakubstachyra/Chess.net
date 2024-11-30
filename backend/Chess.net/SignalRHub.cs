using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    private static string? Player1 = null;
    private static string? Player2 = null;

    public override async Task OnConnectedAsync()
    {
        string clientId = Context.ConnectionId;

        if (Player1 == null)
        {
            Player1 = clientId;
            await Clients.Caller.SendAsync("AssignPlayerColor", "white");
        }
        else if (Player2 == null)
        {
            Player2 = clientId;
            await Clients.Caller.SendAsync("AssignPlayerColor", "black");
            await Clients.All.SendAsync("GameReady");
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Game is full.");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.ConnectionId == Player1)
        {
            Player1 = null;
        }
        else if (Context.ConnectionId == Player2)
        {
            Player2 = null;
        }

        await Clients.All.SendAsync("PlayerDisconnected");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task YourMove()
    {
        string sender = Context.ConnectionId;
        string? recipient = sender == Player1 ? Player2 : Player1;

        if (recipient != null)
        {
            await Clients.Client(recipient).SendAsync("OpponentMoved");
        }
    }
    
}
