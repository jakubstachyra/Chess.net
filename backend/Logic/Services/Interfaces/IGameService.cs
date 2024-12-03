using ChessGame;
using ChessGame.GameMechanics;

namespace Chess.net.Services.Interfaces
{
    public interface IGameService
    {
        public void InitializeGame(int id);
        public List<Move> GetAllPlayerMoves();

        public void MakeSentMove(string move);

        public Move CalculateBlackMove();

        public string SendFen();

        public string WhoToMove();

    }
}
