using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class RankingRepository(DomainDataContext context): Repository<Ranking>(context), IRankingRepository
    {
    }
}
