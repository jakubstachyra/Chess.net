using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.DataRepositories
{
    public class RankingRepository(DomainDataContext context): Repository<Ranking>(context), IRankingRepository
    {
    }
}
