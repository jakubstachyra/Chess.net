using Domain.Common;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using ChessDotNet;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Infrastructure.DataRepositories;
using Domain.DTOs;
using ChessGame.Pieces;

namespace Logic.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IDataRepository _repository;

        public HistoryService(IDataRepository repository)
        {
            _repository = repository;
        }

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
            ChessDotNet.ChessGame chessGame = new ChessDotNet.ChessGame(game.StartFen); // Użycie pozycji startowej z bazy danych
            string lastFen = game.StartFen; // Początkowo ostatni FEN to pozycja startowa

            var moveList = new List<object>();

            foreach (var move in moves)
            {
                string whiteFen = null;
                string blackFen = null;
                string whiteAlgebraic = null;
                string blackAlgebraic = null;

                // Wykonaj ruch białych
                if (!string.IsNullOrEmpty(move.WhiteMove))
                {
                    var whiteMove = new ChessDotNet.Move(
                        move.WhiteMove[..2],
                        move.WhiteMove[2..],
                        Player.White
                    );

                    whiteAlgebraic = GenerateAlgebraicNotation(chessGame, whiteMove);
                    var resultMove = chessGame.MakeMove(whiteMove, true);

                    if (resultMove == MoveType.Invalid)
                    {
                        throw new ArgumentException($"Invalid white move: {move.WhiteMove}");
                    }

                    whiteFen = chessGame.GetFen();
                }

                // Wykonaj ruch czarnych (jeśli istnieje)
                if (!string.IsNullOrEmpty(move.BlackMove))
                {
                    var blackMove = new ChessDotNet.Move(
                        move.BlackMove[..2],
                        move.BlackMove[2..],
                        Player.Black
                    );

                    blackAlgebraic = GenerateAlgebraicNotation(chessGame, blackMove);
                    var resultMove = chessGame.MakeMove(blackMove, true);

                    if (resultMove == MoveType.Invalid)
                    {
                        throw new ArgumentException($"Invalid black move: {move.BlackMove}");
                    }

                    blackFen = chessGame.GetFen();
                }

                lastFen = blackFen ?? whiteFen;

                // Dodaj ruch z FEN-ami i notacją algebraiczną do listy
                moveList.Add(new
                {
                    moveNumber = move.MoveNumber,
                    whiteMove = whiteAlgebraic,
                    blackMove = blackAlgebraic,
                    whiteFen = whiteFen,
                    blackFen = blackFen,
                    whiteRemainingTimeMs = move.WhiteRemainingTimeMs,
                    blackRemainingTimeMs = move.BlackRemainingTimeMs
                });
            }

            // Jeśli tylko podsumowanie
            if (summaryOnly)
            {
                return new
                {
                    gameId = game.Id,
                    startFen = game.StartFen, // Pozycja początkowa
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

            // Zwróć pełne dane gry
            return new
            {
                gameId = game.Id,
                startFen = game.StartFen, // Pozycja początkowa
                whitePlayer = game.WhitePlayer != null
                    ? new { game.WhitePlayer.Id, game.WhitePlayer.UserName }
                    : null,
                blackPlayer = game.BlackPlayer != null
                    ? new { game.BlackPlayer.Id, game.BlackPlayer.UserName }
                    : null,
                result = game.Result,
                movesHistory = moveList
            };
        }

        private string GenerateAlgebraicNotation(ChessDotNet.ChessGame game, ChessDotNet.Move move)
        {
            // Aby nie modyfikować głównego obiektu `game`, kopiujemy aktualną pozycję do gry tymczasowej.
            ChessDotNet.ChessGame tempGame = new ChessDotNet.ChessGame(game.GetFen());

            // Odczytujemy bierkę ze starej pozycji w kopii gry
            var piece = tempGame.GetPieceAt(move.OriginalPosition);
            if (piece == null)
            {
                throw new ArgumentNullException(
                    $"Lack of piece at position {move.OriginalPosition}, cannot generate notation"
                );
            }

            // W chessDotNet można rozpoznać bierkę przez jej znak FEN (GetFenCharacter),
            // np. 'P' = pionek biały, 'p' = pionek czarny, 'N'/'n' = skoczek itp.
            char fenChar = piece.GetFenCharacter();
            char pieceType = char.ToUpper(fenChar);

            // Dopasowujemy litery do standardowej notacji figur
            string pieceChar = pieceType switch
            {
                'P' => "",  // pionek
                'N' => "N", // skoczek
                'B' => "B", // goniec
                'R' => "R", // wieża
                'Q' => "Q", // hetman
                'K' => "K", // król
                _ => ""
            };

            // Sprawdzamy, czy w docelowym polu stoi inna bierka (co może oznaczać bicie).
            var targetPiece = tempGame.GetPieceAt(move.NewPosition);
            bool isCapture = targetPiece != null;

            string captureSymbol = isCapture ? "x" : "";

            // Jeśli to pionek i nastąpiło bicie, dodajemy kolumnę
            if (pieceType == 'P' && isCapture)
            {
                pieceChar = move.OriginalPosition.ToString()[0].ToString();
            }

            // Wykonujemy ruch na kopii gry.
            var moveResult = tempGame.MakeMove(move, true);
            if (moveResult == MoveType.Invalid)
            {
                throw new ArgumentException($"Move {move.OriginalPosition}->{move.NewPosition} is invalid.");
            }

            // Sprawdzamy, czy ruch jest roszadą
            if (move.OriginalPosition.ToString().ToLower() == "e8" && move.NewPosition.ToString().ToLower() == "g8" && pieceType == 'K')
            {
                return "O-O"; // Krótka roszada
            }
            else if (move.OriginalPosition.ToString().ToLower() == "e8" && move.NewPosition.ToString().ToLower() == "c8" && pieceType == 'K')
            {
                return "O-O-O"; // Długa roszada
            }


            // Sprawdzamy, czy jest szach lub mat
            var opponent = (move.Player == Player.White) ? Player.Black : Player.White;
            bool isCheck = tempGame.IsInCheck(opponent);
            bool isCheckmate = tempGame.IsCheckmated(opponent);

            string checkOrMateSymbol = isCheckmate
                ? "#"  // szach-mat
                : (isCheck ? "+" : "");

            // Budujemy finalną notację
            string algebraicMove = $"{pieceChar}{captureSymbol}{move.NewPosition.ToString().ToLower()}{checkOrMateSymbol}";

            return algebraicMove;
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
