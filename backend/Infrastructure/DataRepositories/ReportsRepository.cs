using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class ReportsRepository(DomainDataContext context): Repository<Report>(context), IReportsRepository
    {
    }
}
