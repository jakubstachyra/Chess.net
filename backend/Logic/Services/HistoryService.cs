using Domain.Common;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using ChessDotNet;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Infrastructure.DataRepositories;
using Domain.DTOs;

namespace Logic.Services
{
    public class HistoryService(IDataRepository repository) : IHistoryService
    {
        private readonly IDataRepository _repository = repository;

        public async Task<object> GetGameHistoryByGameID(int ID, bool summaryOnly = false)
        {
            // Pobierz grę na podstawie ID z załadowaniem powiązanych danych
            var game = await _repository.GameRepository
                .Query()
                .Where(g => g.Id == ID)
                .Include(g => g.WhitePlayer)
                .Include(g => g.BlackPlayer)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                throw new ArgumentNullException($"Not found game with ID: {ID}");
            }

            // Pobierz ruchy powiązane z grą
            var moves = await _repository.MoveRepository
                .Query()
                .Where(x => x.GameID == ID)
                .OrderBy(m => m.MoveNumber)
                .ToListAsync();

            if (moves == null || !moves.Any())
            {
                throw new ArgumentNullException($"Not found moves with gameID: {ID}");
            }

            // Inicjalizacja szachownicy
            ChessDotNet.ChessGame chessGame = new ChessDotNet.ChessGame();
            string lastFen = null!;

            // Przetwarzanie ruchów
            foreach (var move in moves)
            {
                if (!string.IsNullOrEmpty(move.WhiteMove))
                {
                    var whiteMove = new ChessDotNet.Move(
                        move.WhiteMove[..2],
                        move.WhiteMove[2..],
                        Player.White
                    );

                    var resultMove = chessGame.MakeMove(whiteMove, true);

                    if (resultMove == MoveType.Invalid)
                    {
                        throw new ArgumentException($"Invalid white move: {move.WhiteMove}");
                    }

                    lastFen = chessGame.GetFen();
                }

                if (!string.IsNullOrEmpty(move.BlackMove))
                {
                    var blackMove = new ChessDotNet.Move(
                        move.BlackMove[..2],
                        move.BlackMove[2..],
                        Player.Black
                    );

                    var resultMove = chessGame.MakeMove(blackMove, true);

                    if (resultMove == MoveType.Invalid)
                    {
                        throw new ArgumentException($"Invalid black move: {move.BlackMove}");
                    }

                    lastFen = chessGame.GetFen();
                }
            }

            // Jeśli tylko podsumowanie
            if (summaryOnly)
            {
                return new
                {
                    gameId = game.Id,
                    whitePlayer = game.WhitePlayer?.UserName,
                    blackPlayer = game.BlackPlayer?.UserName,
                    result = game.Result == "1-0"
                        ? game.WhitePlayer?.UserName
                        : game.Result == "0-1"
                            ? game.BlackPlayer?.UserName
                            : "Draw",
                    lastFen = lastFen
                };
            }

            // Pełne dane gry
            var moveList = moves.Select(m => new
            {
                moveNumber = m.MoveNumber,
                whiteMove = m.WhiteMove,
                blackMove = m.BlackMove,
                whiteRemainingTimeMs = m.WhiteRemainingTimeMs,
                blackRemainingTimeMs = m.BlackRemainingTimeMs
            }).ToList();

            return new
            {
                gameId = game.Id,
                whitePlayer = game.WhitePlayer != null
                    ? new { game.WhitePlayer.Id, game.WhitePlayer.UserName }
                    : null,
                blackPlayer = game.BlackPlayer != null
                    ? new { game.BlackPlayer.Id, game.BlackPlayer.UserName }
                    : null,
                result = game.Result,
                movesHistory = moveList,
                lastFen = lastFen
            };
        }
        public async Task<IEnumerable<object>> GetRecentGamesByPlayerId(string playerId, int limit, int offset)
        {
            // Pobierz listę gier gracza
            var games = await _repository.GameRepository
                .Query()
                .Where(g => g.WhitePlayer.Id == playerId || g.BlackPlayer.Id == playerId)
                .Include(g => g.WhitePlayer)
                .Include(g => g.BlackPlayer)
                .OrderByDescending(g => g.Date) // Sortowanie po dacie
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            // Dla każdej gry użyj funkcji `GetGameHistoryByGameID` w trybie podsumowania
            var gameSummaries = new List<object>();

            foreach (var game in games)
            {
                var summary = await GetGameHistoryByGameID(game.Id, summaryOnly: true);
                gameSummaries.Add(summary);
            }

            return gameSummaries;
        }


    }
}
