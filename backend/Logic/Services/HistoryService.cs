using Domain.Common;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using ChessDotNet;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Logic.Services
{
    public class HistoryService(IDataRepository repository) : IHistoryService
    {
        private readonly IDataRepository _repository = repository;

        public async Task<object> GetGameHistoryByGameID(int ID)
        {
            // Pobierz grę na podstawie ID z załadowaniem powiązanych danych (WhitePlayer, BlackPlayer)
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

            // Pobierz wynik gry
            var gameResult = game.Result;

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
            var moveList = new List<object>();

            foreach (var move in moves)
            {
                string whiteFen = null;
                string blackFen = null;

                // Wykonaj ruch białych
                if (!string.IsNullOrEmpty(move.WhiteMove))
                {
                    var whiteMove = new ChessDotNet.Move(
                        move.WhiteMove[..2], // Pole początkowe
                        move.WhiteMove[2..], // Pole docelowe
                        Player.White
                    );

                    var resultMove = chessGame.MakeMove(whiteMove, true);

                    if (resultMove == MoveType.Invalid)
                    {
                        throw new ArgumentException($"Invalid white move: {move.WhiteMove}");
                    }

                    // Zapisz FEN po ruchu białych
                    whiteFen = chessGame.GetFen();
                }

                // Wykonaj ruch czarnych (jeśli istnieje)
                if (!string.IsNullOrEmpty(move.BlackMove))
                {
                    var blackMove = new ChessDotNet.Move(
                        move.BlackMove[..2], // Pole początkowe
                        move.BlackMove[2..], // Pole docelowe
                        Player.Black
                    );

                    var resultMove = chessGame.MakeMove(blackMove, true);

                    if (resultMove == MoveType.Invalid)
                    {
                        throw new ArgumentException($"Invalid black move: {move.BlackMove}");
                    }

                    // Zapisz FEN po ruchu czarnych
                    blackFen = chessGame.GetFen();
                }

                // Dodaj ruch i FENy do listy
                moveList.Add(new
                {
                    moveNumber = move.MoveNumber,
                    whiteMove = move.WhiteMove,
                    blackMove = move.BlackMove,
                    whiteFen = whiteFen,
                    blackFen = blackFen,
                    whiteRemainingTimeMs = move.WhiteRemainingTimeMs,
                    blackRemainingTimeMs = move.BlackRemainingTimeMs
                });
            }

            // Zwróć pełne dane gry
            return new
            {
                gameId = game.Id,
                whitePlayer = game.WhitePlayer != null
                    ? new { game.WhitePlayer.Id, game.WhitePlayer.UserName }
                    : null,
                blackPlayer = game.BlackPlayer != null
                    ? new { game.BlackPlayer.Id, game.BlackPlayer.UserName }
                    : null,
                result = gameResult,
                movesHistory = moveList
            };
        }
    }
}
