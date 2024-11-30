using Chess.net.Services.Interfaces;
using ChessGame.GameMechanics;
using Domain.Common;
using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.DataRepositories;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Chess.net.Services
{
    public class SaveGameService(IDataRepository repository) : ISaveGameService
    {
        private readonly IDataRepository _repository = repository;

        public async Task<bool> SaveMovesAsync(int gameId, List<ChessGame.GameMechanics.Move> whiteMoves, List<ChessGame.GameMechanics.Move> blackMoves, List<long> whiteTimeMs, List<long> blackTimeMs)
        {
            var game = await _repository.GameRepository.GetByIDAsync(gameId);
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
                await _repository.MoveRepository.AddAsync(dbMove);

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
               await  _repository.MoveRepository.AddAsync(dbMove);
            }

            return true;
        }

        public async Task<(List<string> whiteMoves, List<string> blackMoves)> returnMovesAsync(int gameId)
        {
           
            var moves = await _repository.MoveRepository.GetByConditionAsync(m => m.GameID == gameId);

            
            var orderedMoves = moves.OrderBy(m => m.MoveNumber).ToList();

            var whiteMoves = orderedMoves.Select(m => m.WhiteMove).Where(wm => wm != null).ToList();
            var blackMoves = orderedMoves.Select(m => m.BlackMove).Where(bm => bm != null).ToList();

            return (whiteMoves, blackMoves);
        }
    }
}
