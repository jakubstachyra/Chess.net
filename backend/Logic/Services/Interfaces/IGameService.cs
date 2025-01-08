using ChessGame;
using ChessGame.GameMechanics;

namespace Chess.net.Services.Interfaces
{
    public interface IGameService
    {
        public int InitializeGameWithComputer(string userIdPlayer1);
        public int InitializeGameWithPlayer(string userIdPlayer1 = "guest", string userIdPlayer2 = "guest");

        public List<Move> GetAllPlayerMoves(int gameId);

        public void MakeSentMove(int gameId,string move);

        public Move CalculateComputerMove(int gameId);

        public string SendFen(int gameId);

        public Task<bool> GetGameState(int gameId);
        public void ReceiveFen(int gameId,string FEN);
        public int WhoToMove(int gameId);

    }
}
