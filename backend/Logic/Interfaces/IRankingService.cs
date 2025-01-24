
using Domain;
using Domain.Common;

namespace Logic.Interfaces
{
    public interface IRankingService
    {
        Task<bool> updateRankingByID(string userID, int rankingID, int pointsDelta);
        Task<RankingsUser> getUserRankingByID(string userID, int rankingID);
        Task<IEnumerable<UserRankingDto>> getUserRankingsByID(string userID);
        public Task CalculateDeltaAndUpdateRanking(string userId1, string userId2, string result, string mode);

    }
}
