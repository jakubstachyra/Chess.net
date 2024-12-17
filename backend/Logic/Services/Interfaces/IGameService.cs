using ChessGame;
using ChessGame.GameMechanics;

namespace Chess.net.Services.Interfaces
{
    public interface IGameService
    {
        public int InitializeGame();
        public List<Move> GetAllPlayerMoves(int gameId);

        public void MakeSentMove(int gameId,string move);

        public Move CalculateComputerMove(int gameId);

        public string SendFen(int gameId);

        public int WhoToMove(int gameId);

    }
}
