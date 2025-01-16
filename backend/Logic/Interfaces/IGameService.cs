using ChessGame;
using ChessGame.GameMechanics;
using System.Drawing;

namespace Logic.Interfaces
{
    public interface IGameService
    {
        public Task<int> InitializeGameWithComputer(string userIdPlayer1);
        public Task<int> InitializeGameWithPlayer(string userIdPlayer1 = "guest", string userIdPlayer2 = "guest");

        public List<Move> GetAllPlayerMoves(int gameId);

        public void MakeSentMove(int gameId, string move);

        public Move CalculateComputerMove(int gameId);

        public string SendFen(int gameId);

        public Task<bool> GetGameState(int gameId);
        public void ReceiveFen(int gameId,string FEN);
        public int WhoToMove(int gameId);

        public Task<(bool Success, string Message,int gameId)> AddGameToRepositoryAsync(string whitePlayerId, string blackPlayerId);
        public bool setTimeIsOver(int gameId, string color);
        public bool setGameMode(int gameId, string mode);
        public bool addMoveTime(int gameId, int remainingTime);

        public void setPlayerResigned(int gameId, int winerColor);

        public void setPlayerDrawed(int gameId);
        public Task<bool> GameEnded(int gameId);

        Task<bool> ResignGame(int gameId, string userId);
        Task EndGameAsync(int gameId, string winner, string loser, string reason, bool draw = false);
    }
}
