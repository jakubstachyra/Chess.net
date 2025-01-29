using ChessGame;
using ChessGame.GameMechanics;
using Domain.Common;
using System.Drawing;
using Color = ChessGame.Color;

namespace Logic.Interfaces
{
    public interface IGameService
    {
        public Task<int> InitializeGameWithComputer(string userIdPlayer1);
        public Task<int> InitializeGameWithPlayer(string mode, int timer, string userIdPlayer1 = "guest", string userIdPlayer2 = "guest");

        public List<ChessGame.GameMechanics.Move> GetAllPlayerMoves(int gameId);

        public void MakeSentMove(int gameId, string move);

        public string CalculateComputerMove(int gameId);

        public string SendFen(int gameId);

        public Task<bool> GetGameState(int gameId, bool computer = false);
        public void ReceiveFen(int gameId,string FEN);
        public int WhoToMove(int gameId);

        public void promoteComputerPiece(string move, int gameId);

        public Task<(bool Success, string Message,int gameId)> AddGameToRepositoryAsync(string whitePlayerId, string blackPlayerId, string mode);
        public bool setTimeIsOver(int gameId, string color);
        public bool setGameMode(int gameId, string mode);
        public bool addMoveTime(int gameId, int remainingTime);

        public void setPlayerResigned(int gameId, int winerColor);

        public void setPlayerDrawed(int gameId);
        public Task<bool> GameEnded(int gameId);

        Task<bool> ResignGame(int gameId, string userId);
        Task EndGameAsync(int gameId, string winner, string loser, string reason, bool draw = false, bool computer = false);
        List<MoveHistoryEntry> GetFullMoveHistory(int gameId);
        void AddMoveHistoryEntry(int gameId, string move, string fen, int whiteTimeMs, int blackTimeMs);
        bool TryGetGame(int gameId, out ChessGame.GameMechanics.Game game);

        public string getGameMode(int gameId);

        public Position getNewKingPosition(int gameId, Color color);
    }
}
