using Chess.net.Services.Interfaces;
using ChessGame.GameMechanics;
using Domain.Common;
using Domain.Users;
using Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Chess.net.Services
{
    public class SaveGameService : ISaveGameService
    {
        private readonly DomainDataContext _context;

        public SaveGameService(DomainDataContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveMovesAsync(int gameId, List<ChessGame.GameMechanics.Move> whiteMoves, List<ChessGame.GameMechanics.Move> blackMoves, List<long> whiteTimeMs, List<long> blackTimeMs)
        {
            //DO POPRAWY GAMEID na string wszedzie!!!!
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null)
                return false;
            for(int i=0;i<blackMoves.Count;i++)
            {

                var dbMove = new Domain.Common.Move
                {
                    GameID = gameId,
                    Game = game,
                    WhiteMove = whiteMoves[i].ToString(),
                    BlackMove = blackMoves[i].ToString(),
                    WhiteRemainingTimeMs = whiteTimeMs[i],
                    BlackRemainingTimeMs = blackTimeMs[i],
                    MoveNumber = i + 1,
                };
                _context.Moves.Add(dbMove);

            }
            if(blackMoves.Count <whiteMoves.Count) 
            {
                var dbMove = new Domain.Common.Move
                {
                    GameID = gameId,
                    Game = game,
                    WhiteMove = whiteMoves[blackMoves.Count].ToString(),
                    WhiteRemainingTimeMs = whiteTimeMs[blackMoves.Count],
                    MoveNumber = blackMoves.Count + 1,
                };
                _context.Moves.Add(dbMove);
            }
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(List<string> whiteMoves, List<string> blackMoves)> returnMovesAsync(int gameId)
        {
            var whiteMoves = await _context.Moves.Where(m => m.GameID == gameId).OrderBy(m => m.MoveNumber).Select(m => m.WhiteMove).ToListAsync();
            var blackMoves = await _context.Moves.Where(m => m.GameID == gameId).OrderBy(m => m.MoveNumber).Select(m => m.BlackMove).ToListAsync();

            return (whiteMoves, blackMoves);
        }
    }
}
