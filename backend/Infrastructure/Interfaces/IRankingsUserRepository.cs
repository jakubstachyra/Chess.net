using Domain.Common;

namespace Infrastructure.Interfaces
{
    public interface IRankingsUserRepository: IRepository<RankingsUser>
    {
        Task<IEnumerable<RankingsUser>> GetAllRankingsAsync();
    }
}
