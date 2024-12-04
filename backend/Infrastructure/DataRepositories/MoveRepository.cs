using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class MoveRepository(DomainDataContext context) : Repository<Move>(context), IMoveRepository
    {
    }
}
