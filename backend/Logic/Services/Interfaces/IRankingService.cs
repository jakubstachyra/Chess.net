using Domain.Common;

namespace Logic.Services.Interfaces
{
    public interface IRankingService
    {
        Task<bool > updateRankingByID(string userID, int rankingID, int pointsDelta);
        Task<RankingsUser> getUserRankingByID(string userID, int rankingID);
        Task<IEnumerable<(string Ranking, string RankingInfo, int Points)>> getUserRankingsByID(string userID);
    }
}
