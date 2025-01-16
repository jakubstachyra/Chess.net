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
using ChessGame.GameMechanics;
using Domain.Common;
using ChessGame;

public class GameHub : Hub
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IGameService _gameService;
    private readonly DomainDataContext _domainDataContext;

    // Maps user -> connectionId, and vice versa
    private static readonly ConcurrentDictionary<string, string> UserToConnectionIdMap
        = new ConcurrentDictionary<string, string>();
    private static readonly ConcurrentDictionary<string, string> ConnectionIdToUserMap
        = new ConcurrentDictionary<string, string>();

    // The matchmaking queue: Key = connectionId, Value = (UserId, Ranking, Mode, Timer)
    private static readonly ConcurrentDictionary<string, (string UserId, int Ranking, string Mode, int Timer)> PlayersQueue
        = new ConcurrentDictionary<string, (string, int, string, int)>();

    // The active games: Key = gameId as string, Value = (player1, player2, color1, color2, mode, timer)
    private static readonly ConcurrentDictionary<string, (string Player1Id, string Player2Id,
        string Player1Color, string Player2Color, string Mode, int Timer)> ActiveGames
        = new ConcurrentDictionary<string, (string, string, string, string, string, int)>();

    // Same as ActiveGames but storing **connection IDs** for each player
    private static readonly ConcurrentDictionary<string, (string Player1ConnId, string Player2ConnId,
        string Player1Color, string Player2Color, string Mode, int Timer)> ActiveGamesConnectionIds
        = new ConcurrentDictionary<string, (string, string, string, string, string, int)>();

    // Timers for each player's clock: Key = connectionId, Value = (Timer, RemainingSeconds, gameId)
    private static readonly ConcurrentDictionary<string, (Timer Timer, int RemainingTime, int GameId)> ConnectionTimers
        = new ConcurrentDictionary<string, (Timer, int, int)>();

    // A lock object for queue operations
    private static readonly object _queueLock = new object();

    public GameHub(IGameService gameService,
                   DomainDataContext domainDataContext,
                   IHubContext<GameHub> hubContext)
    {
        _domainDataContext = domainDataContext;
        _gameService = gameService;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        string clientId = Context.ConnectionId;
        string userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated.");
            return;
        }

        ConnectionIdToUserMap[clientId] = userId;
        UserToConnectionIdMap[userId] = clientId;

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string clientId = Context.ConnectionId;
        Console.WriteLine($"OnDisconnectedAsync called. ClientId = {clientId}");

        lock (_queueLock)
        {
            // 1. Remove from queue if present
            PlayersQueue.TryRemove(clientId, out _);

            // 2. If part of an active game, remove it
            var activeGameEntry = ActiveGamesConnectionIds
                .FirstOrDefault(x => x.Value.Player1ConnId == clientId || x.Value.Player2ConnId == clientId);

            if (!string.IsNullOrEmpty(activeGameEntry.Key))
            {
                // Also remove from ActiveGames
                ActiveGames.TryRemove(activeGameEntry.Key, out _);
                ActiveGamesConnectionIds.TryRemove(activeGameEntry.Key, out _);
                Console.WriteLine($"Removed game {activeGameEntry.Key} from ActiveGames.");
            }
        }

        // 3. Remove from user->connection mapping
        string userId;
        if (ConnectionIdToUserMap.TryRemove(clientId, out userId))
        {
            UserToConnectionIdMap.TryRemove(userId, out _);
        }
        Console.WriteLine($"Removed user {userId} from maps.");

        // 4. Stop and remove any running timer
        if (ConnectionTimers.TryRemove(clientId, out var timerData))
        {
            timerData.Timer.Stop();
            timerData.Timer.Dispose();
            Console.WriteLine($"Timer for {clientId} stopped and disposed.");
        }

        await Clients.All.SendAsync("PlayerDisconnected", clientId);
        await BroadcastQueueSize();

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows the front-end to see how many are in queue
    /// </summary>
    private async Task BroadcastQueueSize()
    {
        int queueSize = PlayersQueue.Count;
        await Clients.All.SendAsync("QueueSizeUpdated", queueSize);
    }

    /// <summary>
    /// Called from the client to find an opponent. If found, create a game. If not, keep in queue.
    /// </summary>
    public async Task FindOpponent(string clientId, string mode, int timer)
    {
        string userId = ConnectionIdToUserMap[clientId];

        // Try to get user ranking
        var ranking = await _domainDataContext.RankingsUsers
            .FirstOrDefaultAsync(x => x.UserID == userId);
        int userRanking = ranking?.Points ?? 1000; // default 1000 if no rank found

        lock (_queueLock)
        {
            // Add the caller to the queue
            PlayersQueue[clientId] = (userId, userRanking, mode, timer);
        }

        Console.WriteLine($"ClientId {clientId} is searching for opponent. Queue size: {PlayersQueue.Count}");

        // Attempt to match
        await MatchPlayersIfPossible(clientId, mode, timer);
        await BroadcastQueueSize();
    }

    /// <summary>
    /// Attempts to find an opponent with same mode, timer. If found, creates a new game.
    /// </summary>
    private async Task MatchPlayersIfPossible(string clientId, string mode, int timer)
    {
        // Already removed or not present
        if (!PlayersQueue.TryGetValue(clientId, out var callerData))
            return;

        // The actual matching
        lock (_queueLock)
        {
            // If the queue has no other suitable candidate, do nothing
            var potentialOpponent = PlayersQueue
                .Where(kv =>
                    kv.Key != clientId &&
                    kv.Value.Mode == mode &&
                    kv.Value.Timer == timer)
                // Sort by ranking difference, pick the closest
                .OrderBy(kv => Math.Abs(kv.Value.Ranking - callerData.Ranking))
                .FirstOrDefault();

            if (potentialOpponent.Key == null)
            {
                // No suitable opponent
                // The client remains in the queue
                return;
            }

            // We have an opponent. Remove both from queue
            if (!PlayersQueue.TryRemove(clientId, out _) ||
                !PlayersQueue.TryRemove(potentialOpponent.Key, out _))
            {
                // Something went wrong
                Clients.Caller.SendAsync("Error", "Could not remove from queue. Try again.");
                return;
            }

            // Create the game
            int newGameId = _gameService.InitializeGameWithPlayer(
                callerData.UserId, potentialOpponent.Value.UserId);

            // For simplicity: caller -> white, opponent -> black
            var callerColor = "white";
            var opponentColor = "black";

            ActiveGames[newGameId.ToString()] = (
                callerData.UserId,
                potentialOpponent.Value.UserId,
                callerColor,
                opponentColor,
                mode,
                timer
            );

            // For now, we don't yet know the connectionIDs for each user
            ActiveGamesConnectionIds[newGameId.ToString()] = (
                null,
                null,
                callerColor,
                opponentColor,
                mode,
                timer
            );

            // Let both sides know they have a game
            // They will call "AssignClientIdToGame" next from the front end
            Clients.Client(clientId).SendAsync("GameReady", newGameId);
            Clients.Client(potentialOpponent.Key).SendAsync("GameReady", newGameId);

            Console.WriteLine($"Game {newGameId} created between {clientId} and {potentialOpponent.Key}.");
        }
    }

    /// <summary>
    /// After receiving "GameReady", each client calls this to identify
    /// themselves as Player1 or Player2 in the ActiveGamesConnectionIds dictionary.
    /// </summary>
    public async Task AssignClientIdToGame(string gameId)
    {
        if (!ActiveGames.TryGetValue(gameId, out var gameData))
        {
            await Clients.Caller.SendAsync("Error", "Game not found in ActiveGames.");
            return;
        }

        string clientId = Context.ConnectionId;
        string userId = ConnectionIdToUserMap[clientId];

        if (userId == gameData.Player1Id)
        {
            // This is the first player
            var connData = ActiveGamesConnectionIds[gameId];
            ActiveGamesConnectionIds[gameId] = (
                clientId,
                connData.Player2ConnId,
                connData.Player1Color,
                connData.Player2Color,
                connData.Mode,
                connData.Timer
            );

            // Add the connection to the game group
            await Groups.AddToGroupAsync(clientId, gameId);
        }
        else if (userId == gameData.Player2Id)
        {
            // This is the second player
            var connData = ActiveGamesConnectionIds[gameId];
            ActiveGamesConnectionIds[gameId] = (
                connData.Player1ConnId,
                clientId,
                connData.Player1Color,
                connData.Player2Color,
                connData.Mode,
                connData.Timer
            );

            // Add the connection to the game group
            await Groups.AddToGroupAsync(clientId, gameId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "You are not a player in this game.");
            return;
        }

        // If both players are now assigned, the game can start
        var finalData = ActiveGamesConnectionIds[gameId];
        if (finalData.Player1ConnId != null && finalData.Player2ConnId != null)
        {
            Console.WriteLine($"Game {gameId} is now fully assigned. Notifying players...");
            await NotifyGameIsReady(int.Parse(gameId));
        }
    }


    /// <summary>
    /// Called once both sides have assigned their connection IDs.
    /// Creates timers for each player, notifies them that the game is ready to begin.
    /// </summary>
    private async Task NotifyGameIsReady(int gameId)
    {
        if (!ActiveGamesConnectionIds.TryGetValue(gameId.ToString(), out var connections))
            return; // Nothing to do

        // Create timers for each player
        if (!string.IsNullOrEmpty(connections.Player1ConnId))
            await CreateTimer(connections.Player1ConnId, connections.Timer, gameId);

        if (!string.IsNullOrEmpty(connections.Player2ConnId))
            await CreateTimer(connections.Player2ConnId, connections.Timer, gameId);

        // Now signal to both sides that the game is ready
        if (connections.Player1ConnId != null && connections.Player2ConnId != null)
        {
            await Clients.Client(connections.Player1ConnId).SendAsync("GameIsReady", gameId);
            await Clients.Client(connections.Player2ConnId).SendAsync("GameIsReady", gameId);

            Console.WriteLine($"Game {gameId} is ready for both players.");
        }
    }
    /// <summary>
    /// Get user move
    /// </summary>
    public async Task ReceiveMoveAsync(int gameId, string move)
    {
        if (string.IsNullOrEmpty(move))
        {
            await Clients.Caller.SendAsync("Error", "Move is required.");
            return;
        }

        try
        {
            // Pobierz grê
            if (!_gameService.TryGetGame(gameId, out var game))
            {
                await Clients.Caller.SendAsync("Error", "Game not found.");
                return;
            }

            // Przed wykonaniem ruchu, wygeneruj notacjê algebraiczn¹
            Position start = ChessGame.Utils.Converter.ChessNotationToPosition(move.Substring(0, 2));
            Position end = ChessGame.Utils.Converter.ChessNotationToPosition(move.Substring(2, 2));
            var moveObj = new ChessGame.GameMechanics.Move(start, end);
            string algebraic = game.chessBoard.GenerateAlgebraicNotation(game.chessBoard, moveObj);

            // Wykonaj ruch
            _gameService.MakeSentMove(gameId, move);

            var currentFen = _gameService.SendFen(gameId);
            var (whiteConnId, blackConnId, _, _, _, _) = ActiveGamesConnectionIds[gameId.ToString()];

            // Pobierz aktualny czas dla obu graczy
            int whiteTimeMs = 0, blackTimeMs = 0;
            if (!string.IsNullOrEmpty(whiteConnId) && ConnectionTimers.TryGetValue(whiteConnId, out var whiteTimerData))
            {
                whiteTimeMs = whiteTimerData.RemainingTime * 1000;
            }
            if (!string.IsNullOrEmpty(blackConnId) && ConnectionTimers.TryGetValue(blackConnId, out var blackTimerData))
            {
                blackTimeMs = blackTimerData.RemainingTime * 1000;
            }

            // Dodaj wpis do historii z wygenerowan¹ notacj¹
            _gameService.AddMoveHistoryEntry(gameId, algebraic, currentFen, whiteTimeMs, blackTimeMs);

            var fullHistory = _gameService.GetFullMoveHistory(gameId);
            await Clients.Group(gameId.ToString())
                .SendAsync("MoveHistoryUpdated", fullHistory);

            await Clients.Group(gameId.ToString()).SendAsync("MoveAcknowledged", $"Move {move} received for game {gameId}");
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }
    

    /// <summary>
    /// Called by the moving player to indicate they finished their move.
    /// The server stops their timer, and starts the opponent's timer.
    /// The opponent is notified via "OpponentMoved".
    /// </summary>
    public async Task YourMove(string gameId)
    {
        string senderConnId = Context.ConnectionId;

        // Stop the sender's clock
        await StopTimer(senderConnId);

        // Find the opponent and start their clock
        var opponent = FindOpponentConnectionId(gameId, senderConnId);

        if (!string.IsNullOrEmpty(opponent))
        {
            // Start opponent's clock
            await StartTimer(opponent);
            await Clients.Client(opponent).SendAsync("OpponentMoved");
        }
    }

    /// <summary>
    /// Returns the opponent's connectionId for a given gameId and player connectionId.
    /// </summary>
    private string? FindOpponentConnectionId(string gameId, string connectionId)
    {
        if (ActiveGamesConnectionIds.TryGetValue(gameId, out var game))
        {
            if (connectionId == game.Player1ConnId)
                return game.Player2ConnId;
            else if (connectionId == game.Player2ConnId)
                return game.Player1ConnId;
        }
        return null;
    }

    /// <summary>
    /// Allows the client to leave the queue if they no longer want to wait.
    /// </summary>
    public async Task RemovePlayerFromQueue(string clientId)
    {
        if (PlayersQueue.TryRemove(clientId, out _))
        {
            await Clients.Client(clientId).SendAsync("QueueLeft", "You have left the queue.");
            await Clients.All.SendAsync("PlayerLeftQueue", clientId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Could not remove from queue. Possibly not in queue.");
        }

        await BroadcastQueueSize();
    }

    /// <summary>
    /// Returns "white" or "black" for the caller's assigned color in a given game.
    /// </summary>
    public string GetPlayerColor(string gameId)
    {
        if (ActiveGamesConnectionIds.TryGetValue(gameId, out var game))
        {
            if (game.Player1ConnId == Context.ConnectionId)
                return game.Player1Color;
            else if (game.Player2ConnId == Context.ConnectionId)
                return game.Player2Color;
        }
        return null; // Not found
    }

    /// <summary>
    /// Creates a 1-second interval timer for the given connection. The 'time' parameter is total seconds.
    /// </summary>
    private async Task CreateTimer(string connectionId, int time, int gameId)
    {
        if (!ConnectionTimers.ContainsKey(connectionId))
        {
            var timer = new Timer(1000); // fires every second
            var remainingTime = time;

            timer.Elapsed += async (sender, e) =>
                await HandlePlayerTimerElapsed(sender, e, connectionId);

            ConnectionTimers[connectionId] = (timer, remainingTime, gameId);
        }
    }

    /// <summary>
    /// Starts the player's clock (timer).
    /// </summary>
    public async Task StartTimer(string connectionId)
    {
        if (ConnectionTimers.TryGetValue(connectionId, out var timerData))
            timerData.Timer.Start();
    }

    /// <summary>
    /// Stops the player's clock (timer).
    /// </summary>
    public async Task StopTimer(string connectionId)
    {
        if (ConnectionTimers.TryGetValue(connectionId, out var timerData))
            timerData.Timer.Stop();
    }
    /// <summary>
    /// Gives userId from connectionID
    /// </summary>
    public string GetUserIdByConnectionId(string connectionId)
    {
        if (ConnectionIdToUserMap.TryGetValue(connectionId, out var userId))
        {
            return userId;
        }
        return null; 
    }

    /// <summary>
    /// Decrements the player's remaining time each second. If time hits 0, end the game.
    /// </summary>
    private async Task HandlePlayerTimerElapsed(object sender, ElapsedEventArgs e, string connectionId)
    {
        if (ConnectionTimers.TryGetValue(connectionId, out var data))
        {
            var (timer, remainingTime, gameId) = data;
            if (remainingTime > 0)
            {
                remainingTime--;
                ConnectionTimers[connectionId] = (timer, remainingTime, gameId);
                await BroadcastGameTimers(connectionId);
            }
            else
            {
                // Time's up
                await GameEnded(gameId, connectionId);

                timer.Stop();
                timer.Dispose();
            }
        }
    }
    public async Task DrawProposed(int gameId)
    {
        string callerConnId = Context.ConnectionId;
        // ZnajdŸ po³¹czenie przeciwnika w tej grze
        string? opponentConnId = FindOpponentConnectionId(gameId.ToString(), callerConnId);

        if (!string.IsNullOrEmpty(opponentConnId))
        {
            await Clients.Client(opponentConnId).SendAsync("DrawProposed");
        }
    }

    public async Task DrawAccept(int gameId)
    {
        await _gameService.EndGameAsync(gameId, "", "", "Draw acceptance", true);
    }
    public async Task DrawRejected(int gameId)
    {
        await _hubContext.Clients.Group(gameId.ToString()).SendAsync("DrawRejected");
    }
    /// <summary>
    /// Called if a player's time runs out or if the game is otherwise ended.
    /// </summary>
    public async Task GameEnded(int gameId, string connectionId)
    {
        Console.WriteLine($"GameEnded called for game {gameId}, caused by {connectionId}");

        if (ActiveGamesConnectionIds.TryGetValue(gameId.ToString(), out var game))
        {
            // figure out which color ran out of time
            string color = (game.Player1ConnId == connectionId) ? game.Player1Color : game.Player2Color;
            bool isOver = _gameService.setTimeIsOver(gameId, color);

            if (isOver)
            {
                // send final signals to both players
                string loserConnId = connectionId;
                string winnerConnId = (loserConnId == game.Player1ConnId)
                    ? game.Player2ConnId
                    : game.Player1ConnId;
                
                await _gameService.EndGameAsync(gameId, GetUserIdByConnectionId(loserConnId),
                    GetUserIdByConnectionId(winnerConnId), "By time");
            }

            // remove from active games
            ActiveGames.TryRemove(gameId.ToString(), out _);
            ActiveGamesConnectionIds.TryRemove(gameId.ToString(), out _);

            // remove timers for both players
            foreach (var pid in new[] { game.Player1ConnId, game.Player2ConnId })
            {
                if (!string.IsNullOrEmpty(pid) && ConnectionTimers.TryRemove(pid, out var tData))
                {
                    tData.Timer.Stop();
                    tData.Timer.Dispose();
                    Console.WriteLine($"Timer for {pid} stopped & disposed.");
                }
            }

            // call gameEnded in your gameService -> triggers DB save, etc.
            await _gameService.GameEnded(gameId);
        }
    }

    /// <summary>
    /// Broadcasts the updated times for both players in a game, so they see a live clock.
    /// </summary>
    private async Task BroadcastGameTimers(string changedConnId)
    {
        string gameId = GetGameIdByPlayerId(changedConnId);
        if (string.IsNullOrEmpty(gameId)) return;

        // The opponent
        var oppConnId = FindOpponentConnectionId(gameId, changedConnId);
        if (string.IsNullOrEmpty(oppConnId)) return;

        // If both players have timers
        if (ConnectionTimers.TryGetValue(changedConnId, out var p1Data) &&
            ConnectionTimers.TryGetValue(oppConnId, out var p2Data))
        {
            // figure out which connection is "white" / "black"
            var (p1ConnId, p2ConnId, p1Color, p2Color, _, _) = ActiveGamesConnectionIds[gameId];

            int whiteTime = (p1ConnId == changedConnId && p1Color == "white")
                ? p1Data.RemainingTime
                : (p2ConnId == changedConnId && p2Color == "white" ? p1Data.RemainingTime : p2Data.RemainingTime);

            int blackTime = (p1ConnId == changedConnId && p1Color == "black")
                ? p1Data.RemainingTime
                : (p2ConnId == changedConnId && p2Color == "black" ? p1Data.RemainingTime : p2Data.RemainingTime);

            // Send to both players
            await _hubContext.Clients.Client(p1ConnId).SendAsync("UpdateTimers", whiteTime, blackTime);
            await _hubContext.Clients.Client(p2ConnId).SendAsync("UpdateTimers", whiteTime, blackTime);
        }
    }

    /// <summary>
    /// Finds the gameId in which this player (connectionId) is participating.
    /// </summary>
    public static string GetGameIdByPlayerId(string connectionId)
    {
        foreach (var kvp in ActiveGamesConnectionIds)
        {
            var gameId = kvp.Key;
            var data = kvp.Value;
            if (data.Player1ConnId == connectionId || data.Player2ConnId == connectionId)
                return gameId;
        }
        return null;
    }
}
