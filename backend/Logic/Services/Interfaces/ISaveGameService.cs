namespace Chess.net.Services.Interfaces
{
    public interface ISaveGameService
    {
        public Task<bool> SaveMovesAsync(int gameId, List<ChessGame.GameMechanics.Move> whiteMoves, List<ChessGame.GameMechanics.Move> blackMoves
, List<long> whiteTimeMs, List<long> blackTimeMs);
        public Task<(List<string> whiteMoves, List<string> blackMoves)> returnMovesAsync(int gameId);
    }
}
