using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class RankingsUserRepository(DomainDataContext context): Repository<RankingsUser>(context), IRankingsUserRepository
    {
    }
}
