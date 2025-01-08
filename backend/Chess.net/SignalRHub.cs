using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;    
using System.Security.Claims;
using System.Collections.Concurrent;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class GameHub : Hub
{
    // Store a mapping between UserId and their respective ConnectionId
    //private static readonly ConcurrentDictionary<string, string> UserToConnectionIdMap = new ConcurrentDictionary<string, string>();

    //// Queue of players waiting for an opponent
    //private static readonly ConcurrentDictionary<string, (string UserId, int Ranking)> PlayersQueue = new ConcurrentDictionary<string, (string, int)>();

    //// Active games with their associated players
    //private static readonly ConcurrentDictionary<string, (string Player1Id, string Player2Id, string Player1Color, string Player2Color)> ActiveGames
    //    = new ConcurrentDictionary<string, (string, string, string, string)>();

    //private readonly IGameService _gameService;
    //private readonly DomainDataContext _domainDataContext;
    //private readonly object _queueLock = new object();

    //public GameHub(IGameService gameService, DomainDataContext domainDataContext)
    //{
    //    _domainDataContext = domainDataContext;
    //    _gameService = gameService;
    //}

    //// Handle when a client connects
    //public override async Task OnConnectedAsync()
    //{
    //    string clientId = Context.ConnectionId;
    //    Console.WriteLine($"Client connected: {clientId}");

    //    var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (userId == null)
    //    {
    //        await Clients.Caller.SendAsync("Error", "User not authenticated.");
    //        return;
    //    }

    //    var ranking = await _domainDataContext.RankingsUsers.FirstOrDefaultAsync(x => x.UserID == userId);
    //    if (ranking == null)
    //    {
    //        await Clients.Caller.SendAsync("Error", "User ranking not found.");
    //        return;
    //    }

    //    // Register the new connection ID for the user
    //    UserToConnectionIdMap[userId] = clientId;

    //    // Add the player to the queue
    //    if (!PlayersQueue.TryAdd(clientId, (userId, ranking.Points)))
    //    {
    //        await Clients.Caller.SendAsync("Error", "Unable to map player to connection.");
    //        return;
    //    }

    //    await BroadcastQueueSize();
    //    await base.OnConnectedAsync();
    //}

    //// Handle when a client disconnects
    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    string clientId = Context.ConnectionId;

    //    lock (_queueLock)
    //    {
    //        PlayersQueue.TryRemove(clientId, out _);

    //        var gameId = ActiveGames.FirstOrDefault(x => x.Value.Player1Id == clientId || x.Value.Player2Id == clientId).Key;
    //        if (!string.IsNullOrEmpty(gameId))
    //        {
    //            ActiveGames.TryRemove(gameId, out _);
    //        }
    //    }

    //    // Remove the user from the connection map
    //    var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (!string.IsNullOrEmpty(userId))
    //    {
    //        UserToConnectionIdMap.TryRemove(userId, out _);
    //    }

    //    await Clients.All.SendAsync("PlayerDisconnected", clientId);
    //    await BroadcastQueueSize();
    //    await base.OnDisconnectedAsync(exception);
    //}

    //// Broadcast the current size of the player queue to all clients
    //private async Task BroadcastQueueSize()
    //{
    //    int queueSize = PlayersQueue.Count;
    //    Console.WriteLine($"Queue size: {queueSize}");
    //    await Clients.All.SendAsync("WaitingForOpponent", queueSize);
    //}

    //// Try to find an opponent for the current player and assign colors
    //private async Task FindOpponentAndAssignColors(string clientId)
    //{
    //    if (!PlayersQueue.TryGetValue(clientId, out var playerData))
    //    {
    //        await Clients.Caller.SendAsync("Error", "Player not found in queue.");
    //        return;
    //    }

    //    string userId = playerData.UserId;
    //    int ranking = playerData.Ranking;

    //    lock (_queueLock)
    //    {
    //        var potentialOpponent = PlayersQueue
    //            .Where(kv => kv.Key != clientId)
    //            .OrderBy(kv => Math.Abs(kv.Value.Ranking - ranking))
    //            .FirstOrDefault();

    //        if (potentialOpponent.Key == null)
    //        {
    //            return; // No opponent found
    //        }

    //        string opponentClientId = potentialOpponent.Key;
    //        string opponentUserId = potentialOpponent.Value.UserId;

    //        // Remove both players from the queue
    //        PlayersQueue.TryRemove(clientId, out _);
    //        PlayersQueue.TryRemove(opponentClientId, out _);

    //        int gameId = _gameService.InitializeGame();

    //        // Randomly assign colors
    //        var random = new Random();
    //        bool callerPlaysWhite = random.Next(0, 2) == 0;

    //        string callerColor = callerPlaysWhite ? "white" : "black";
    //        string opponentColor = callerPlaysWhite ? "black" : "white";

    //        // Add the game to ActiveGames with player colors
    //        ActiveGames.TryAdd(gameId.ToString(), (userId, opponentUserId, callerColor, opponentColor));

    //        // Notify players
    //        Clients.Client(clientId).SendAsync("AssignPlayerColor", callerColor);
    //        Clients.Client(opponentClientId).SendAsync("AssignPlayerColor", opponentColor);

    //        Clients.Client(clientId).SendAsync("GameReady", gameId);
    //        Clients.Client(opponentClientId).SendAsync("GameReady", gameId);
    //    }
    //}

    //public async Task FindOpponent(string clientId)
    //{
    //    await FindOpponentAndAssignColors(clientId);
    //}

    //// Handle a move made by the player
    //public async Task YourMove()
    //{
    //    string sender = Context.ConnectionId;

    //    var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    //    var gameId = ActiveGames.FirstOrDefault(x => x.Value.Player1Id == userId || x.Value.Player2Id == userId).Key;

    //    if (!string.IsNullOrEmpty(gameId))
    //    {
    //        var game = ActiveGames[gameId];
    //        Console.WriteLine($"player1: {game.Player1Id} player2:{game.Player2Id}");
    //        foreach(var a in ActiveGames)
    //        {
    //            Console.WriteLine($"key {a.Key} id1 {a.Value.Player1Id}, id2 {a.Value.Player2Id}");
    //        }
    //        var connectionId1 = UserToConnectionIdMap[game.Player1Id];
    //        var connectionId2 = UserToConnectionIdMap[game.Player2Id];

    //        Console.WriteLine($"sender {sender}, connectionid1 {connectionId1} conecitonid2 {connectionId2}");
    //        string recipient = sender == connectionId1 ? connectionId2 : connectionId1;



    //        if (!string.IsNullOrEmpty(recipient))
    //        {
    //            Console.WriteLine($"wysyłam do {recipient}");
    //            await Clients.Client(recipient).SendAsync("OpponentMoved");
    //        }
    //    }
    //    else
    //    {
    //        await Clients.Caller.SendAsync("Error", "You are not part of any active game.");
    //    }
    //}

    //// Handle removing a player from the queue
    //public async Task RemovePlayerFromQueue(string clientId)
    //{
    //    if (PlayersQueue.TryRemove(clientId, out _))
    //    {
    //        await Clients.Client(clientId).SendAsync("QueueLeft", "You have been removed from the queue.");
    //        await Clients.All.SendAsync("PlayerLeftQueue", clientId);
    //    }
    //    else
    //    {
    //        await Clients.Caller.SendAsync("Error", "Player not found in the queue.");
    //    }

    //    await BroadcastQueueSize();
    //}

    //public async Task GetGameState()
    //{
    //    var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    //    if (string.IsNullOrEmpty(userId))
    //    {
    //        await Clients.Caller.SendAsync("Error", "User not authenticated.");
    //        return;
    //    }

    //    var activeGame = ActiveGames.FirstOrDefault(game =>
    //        game.Value.Player1Id == userId || game.Value.Player2Id == userId);

    //    if (activeGame.Key == null)
    //    {
    //        await Clients.Caller.SendAsync("Error", "No active game found.");
    //        return;
    //    }

    //    var gameId = activeGame.Key;
    //    var (player1Id, player2Id, player1Color, player2Color) = activeGame.Value;

    //    string color = player1Id == userId ? player1Color : player2Color;
    //    Console.WriteLine($"id: {player1Id} color {color}");
    //    await Clients.Caller.SendAsync("GameState", color);
    //}

}
