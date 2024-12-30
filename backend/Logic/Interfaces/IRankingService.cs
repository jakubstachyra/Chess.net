
using Domain;
using Domain.Common;

namespace Logic.Interfaces
{
    public interface IRankingService
    {
        Task<bool> updateRankingByID(string userID, int rankingID, int pointsDelta);
        Task<RankingsUser> getUserRankingByID(string userID, int rankingID);
        Task<IEnumerable<UserRankingDto>> getUserRankingsByID(string userID);
    }
}
