using Domain.Common;

namespace Logic.Interfaces
{
    public interface IHistoryService
    {
        Task<Object> GetGameHistoryByGameID(int ID, bool summaryOnly = false);
        Task<IEnumerable<object>> GetRecentGamesByPlayerId(string playerId, int limit, int offset);
    }
}
