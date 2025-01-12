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
using Logic.Interfaces;
using System.Timers;
using Timer = System.Timers.Timer;
using Domain.Common;
using System.Drawing;


public class GameHub : Hub
{

    private readonly IHubContext<GameHub> _hubContext;
    private readonly IGameService _gameService;
    private readonly DomainDataContext _domainDataContext;
    private readonly object _queueLock = new object();

    private static readonly ConcurrentDictionary<string, string> UserToConnectionIdMap = new ConcurrentDictionary<string, string>();
    private static readonly ConcurrentDictionary<string, string> ConnectionIdMapToUserId = new ConcurrentDictionary<string, string>();


    private static readonly ConcurrentDictionary<string, (string UserId, int Ranking, string mode, int timer)> PlayersQueue = new ConcurrentDictionary<string, (string, int, string, int)>();

    private static readonly ConcurrentDictionary<string, (string Player1Id, string Player2Id, string Player1Color, string Player2Color, string mode, int timer)> ActiveGames
        = new ConcurrentDictionary<string, (string, string, string, string, string, int)>();
    private static readonly ConcurrentDictionary<string, (string Player1Id, string Player2Id, string Player1Color, string Player2Color, string mode, int timer)> ActiveGamesConnectionIds
    = new ConcurrentDictionary<string, (string, string, string, string, string, int)>();

    private static readonly ConcurrentDictionary<string, (Timer Timer, int RemainingTime, int gameId)> ConnectionTimers = new ConcurrentDictionary<string, (Timer, int, int)>();

    public GameHub(IGameService gameService, DomainDataContext domainDataContext, IHubContext<GameHub> hubContext)
    {
        _domainDataContext = domainDataContext;
        _gameService = gameService;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        string clientId = Context.ConnectionId;
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated.");
            return;
        }

        ConnectionIdMapToUserId[clientId] = userId;
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string clientId = Context.ConnectionId;

        lock (_queueLock)
        {
            PlayersQueue.TryRemove(clientId, out _);

            var gameId = ActiveGamesConnectionIds.FirstOrDefault(x =>
                x.Value.Player1Id == clientId || x.Value.Player2Id == clientId).Key;

            if (!string.IsNullOrEmpty(gameId))
            {
                ActiveGames.TryRemove(gameId, out _);
                ActiveGamesConnectionIds.TryRemove(gameId, out _);
            }
        }

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            UserToConnectionIdMap.TryRemove(userId, out _);
            ConnectionIdMapToUserId.TryRemove(clientId, out _);
        }

        if (ConnectionTimers.TryRemove(clientId, out var timerData))
        {
            timerData.Timer.Stop();
            timerData.Timer.Dispose();
        }

        await Clients.All.SendAsync("PlayerDisconnected", clientId);
        await BroadcastQueueSize();
        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastQueueSize()
    {
        int queueSize = PlayersQueue.Count;
        await Clients.All.SendAsync("WaitingForOpponent", queueSize);
    }

    private async Task FindOpponentAndAssignColors(string clientId)
    {
        if (!PlayersQueue.TryGetValue(clientId, out var playerData))
        {
            await Clients.Caller.SendAsync("Error", "Player not found in queue.");
            return;
        }
        Console.WriteLine(Context.ConnectionId);

        lock (_queueLock)
        {
            string userId = playerData.UserId;
            int ranking = playerData.Ranking;
            string mode = playerData.mode;
            int timer = playerData.timer;

            // Search for a suitable opponent
            var potentialOpponent = PlayersQueue
                .Where(kv => kv.Key != clientId && kv.Value.UserId != userId && kv.Value.mode == mode && kv.Value.timer == timer)
                .OrderBy(kv => Math.Abs(kv.Value.Ranking - ranking))
                .FirstOrDefault();

            if (potentialOpponent.Key == null)
            {
                // No suitable opponent found
                return;
            }

            // Safely remove both players from the queue
            string opponentClientId = potentialOpponent.Key;
            string opponentUserId = potentialOpponent.Value.UserId;
            Console.WriteLine($"moje id {clientId} idopcia: {opponentClientId}");
            Console.WriteLine($"compare: {string.Compare(clientId, opponentClientId)}");
            if (string.Compare(clientId, opponentClientId) > 0)
            {
                // This player should not create the game (wait for opponent)
                return;
            }

            PlayersQueue.TryRemove(clientId, out _);
            PlayersQueue.TryRemove(opponentClientId, out _);

            // Create the game
            int gameId = _gameService.InitializeGameWithPlayer(userId, opponentUserId);
            //_gameService.setGameMode(gameId, mode);

            // Assign colors
            string callerColor = "white";
            string opponentColor = "black";

            ActiveGames.TryAdd(gameId.ToString(), (userId, opponentUserId, callerColor, opponentColor, mode, timer));
            ActiveGamesConnectionIds.TryAdd(gameId.ToString(), (null, null, null, null, mode, timer));

            // Notify both players
            Clients.Client(clientId).SendAsync("GameReady", gameId);
            Clients.Client(opponentClientId).SendAsync("GameReady", gameId);
        }
    }

    public async Task FindOpponent(string clientId, string mode, int timer)
    {
        var userId = ConnectionIdMapToUserId[clientId];
        var ranking = await _domainDataContext.RankingsUsers.FirstOrDefaultAsync(x => x.UserID == userId);

        if (!PlayersQueue.TryAdd(clientId, (userId, ranking.Points, mode, timer)))
        {
            await Clients.Caller.SendAsync("Error", "Unable to map player to connection.");
            return;
        }
        await FindOpponentAndAssignColors(clientId);
    }
    public async Task AssignClientIdToGame(string gameId)
    {
        var userId = ConnectionIdMapToUserId[Context.ConnectionId];
        var game = ActiveGames[gameId];
        if (userId == game.Player1Id)
        {
            ActiveGamesConnectionIds[gameId] = (Context.ConnectionId, ActiveGamesConnectionIds[gameId].Player2Id, game.Player1Color,
                ActiveGamesConnectionIds[gameId].Player2Color, ActiveGamesConnectionIds[gameId].mode, ActiveGamesConnectionIds[gameId].timer);
        }
        if (userId == game.Player2Id)
        {
            ActiveGamesConnectionIds[gameId] = (ActiveGamesConnectionIds[gameId].Player1Id, Context.ConnectionId, ActiveGamesConnectionIds[gameId].Player1Color, game.Player2Color, ActiveGamesConnectionIds[gameId].mode, ActiveGamesConnectionIds[gameId].timer);
        }

        if (ActiveGamesConnectionIds[gameId].Player1Id != null && ActiveGamesConnectionIds[gameId].Player2Id != null)
        {
            await NotifyGameIsReady(int.Parse(gameId));
        }
    }


    public async Task NotifyGameIsReady(int gameId)
    {
        if (ActiveGamesConnectionIds.TryGetValue(gameId.ToString(), out var connections))
        {
            await CreateTimer(connections.Player1Id, connections.timer, gameId);
            await CreateTimer(connections.Player2Id, connections.timer, gameId);

            if (connections.Player1Id != null && connections.Player2Id != null)
            {
                await Clients.Client(connections.Player1Id).SendAsync("GameIsReady", gameId);
                await Clients.Client(connections.Player2Id).SendAsync("GameIsReady", gameId);
            }
            else
            {
                Console.WriteLine($"Game {gameId} is not fully initialized yet.");
            }
        }
        else
        {
            Console.WriteLine($"Game {gameId} not found in ActiveGamesConnectionIds.");
        }
    }

    public async Task YourMove(string gameId)
    {
        string sender = Context.ConnectionId;

        if (!string.IsNullOrEmpty(gameId))
        {



            await StopTimer(sender);
            var opponentConnectionId = FindOpponentConnectionId(gameId, sender);

            if (!string.IsNullOrEmpty(opponentConnectionId))
            {
                await StartTimer(opponentConnectionId);
                await Clients.Client(opponentConnectionId).SendAsync("OpponentMoved");
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "You are not part of any active game.");
        }
    }

    private string? FindOpponentConnectionId(string gameId, string connectionId)
    {
        if (ActiveGamesConnectionIds.TryGetValue(gameId, out var game))
        {
            if (connectionId == game.Player1Id) return game.Player2Id;
            else return game.Player1Id;
        }
        return null;
    }

    public async Task RemovePlayerFromQueue(string clientId)
    {
        if (PlayersQueue.TryRemove(clientId, out _))
        {
            await Clients.Client(clientId).SendAsync("QueueLeft", "You have been removed from the queue.");
            await Clients.All.SendAsync("PlayerLeftQueue", clientId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Player not found in the queue.");
        }

        await BroadcastQueueSize();
    }


    public string GetPlayerColor(string gameId)
    {
        var playerConnectionId = Context.ConnectionId;
        if (ActiveGamesConnectionIds.ContainsKey(gameId))
        {
            var game = ActiveGamesConnectionIds[gameId];
            if (game.Player1Id == playerConnectionId)
            {
                return game.Player1Color;
            }
            else if (game.Player2Id == playerConnectionId)
            {
                return game.Player2Color;
            }
        }

        return null;
    }


    private async Task HandlePlayerTimerElapsed(object sender, ElapsedEventArgs e, string connectionId)
    {
        if (ConnectionTimers.ContainsKey(connectionId))
        {
            var (timer, remainingTime, gameId) = ConnectionTimers[connectionId];

            if (remainingTime > 0)
            {
                remainingTime--;

                ConnectionTimers[connectionId] = (timer, remainingTime,gameId);
                
                await BroadcastGameTimers(connectionId);

            }
            else
            {
                await GameEnded(gameId);

                timer.Stop();
            }
        }
    }

    public async Task GameEnded(int gameId)
    {
        var connectionId =Context.ConnectionId;
        if (ActiveGamesConnectionIds.TryGetValue(gameId.ToString(), out var game))
        {
            string color = game.Player1Id == connectionId ? game.Player1Color : game.Player2Color;

            bool isOver = _gameService.setTimeIsOver(gameId, color);

            if (isOver)
            {
                string losingPlayer = connectionId;
                string winningPlayer = FindOpponentConnectionId(gameId.ToString(), connectionId);

  
                await Clients.Client(losingPlayer).SendAsync("TimeOver", color);
                await Clients.Client(winningPlayer).SendAsync("OpponentTimeOver", color);

            }
        }
        _gameService.AddGameToRepositoryAsync(gameId);
    }
    public async Task CreateTimer(string connectionId, int time, int gameId)
    {
        if (!ConnectionTimers.ContainsKey(connectionId))
        {
            var timer = new Timer(1000); // 1-second interval
            var remainingTime = time; // Initial time for the player

            timer.Elapsed += async (sender, e) =>
                await HandlePlayerTimerElapsed(sender, e, connectionId);

            // Add the timer and remaining time to the dictionary
            ConnectionTimers[connectionId] = (timer, remainingTime,gameId);
        }
    }
    public async Task StartTimer(string connectionId)
    {
        //if (!ConnectionTimers.ContainsKey(connectionId))
        //{
        //    var timer = new Timer(1000); // 1-second interval
        //    var remainingTime = time; // Initial time for the player

        //    timer.Elapsed += async (sender, e) =>
        //        await HandlePlayerTimerElapsed(sender, e, connectionId);

        //    // Add the timer and remaining time to the dictionary
        //    ConnectionTimers[connectionId] = (timer, remainingTime);
        //}

        ConnectionTimers[connectionId].Timer.Start();
    }


    public async Task StopTimer(string connectionId)
    {
        if (ConnectionTimers.TryGetValue(connectionId, out var timerData))
        {
            timerData.Timer.Stop();
        }
    }
    private async Task BroadcastGameTimers(string connectionId)
    {
        string gameId = GetGameIdByPlayerId(connectionId);
        var opponent = FindOpponentConnectionId(gameId, connectionId);

        if (!string.IsNullOrEmpty(gameId) && ConnectionTimers.ContainsKey(connectionId) && ConnectionTimers.ContainsKey(opponent))
        {
            var player1Time = ConnectionTimers[connectionId].RemainingTime;
            var player2Time = ConnectionTimers[opponent].RemainingTime;

            // Determine white and black players based on their colors in the game
            var gameData = ActiveGamesConnectionIds[gameId];
            var whitePlayerId = gameData.Player1Color == "white" ? gameData.Player1Id : gameData.Player2Id;
            var blackPlayerId = gameData.Player1Color == "black" ? gameData.Player1Id : gameData.Player2Id;

            var whiteTimer = whitePlayerId == connectionId ? player1Time : player2Time;
            var blackTimer = blackPlayerId == connectionId ? player1Time : player2Time;

            // Ensure the correct sequence: white timer, then black timer
            await _hubContext.Clients.Client(connectionId).SendAsync("UpdateTimers", whiteTimer, blackTimer);
            await _hubContext.Clients.Client(opponent).SendAsync("UpdateTimers", whiteTimer, blackTimer);
        }
    }


    public static string GetGameIdByPlayerId(string playerId)
    {
        foreach (var kvp in ActiveGamesConnectionIds)
        {
            var gameId = kvp.Key;
            var gameData = kvp.Value;

            if (gameData.Player1Id == playerId || gameData.Player2Id == playerId)
            {
                return gameId;
            }
        }

        // Return null if the playerId is not found
        return null;
    }

    //private async Task EndGame(string gameId, string winnerId, string reason)
    //{
    //    // Remove timers and notify clients
    //    if (GameTimers.TryRemove(gameId, out var timers))
    //    {
    //        timers.Player1Timer.Stop();
    //        timers.Player2Timer.Stop();
    //    }

    //    ActiveGames.TryRemove(gameId, out _);

    //    await Clients.Group(gameId).SendAsync("GameEnded", winnerId, reason);
    //}


}