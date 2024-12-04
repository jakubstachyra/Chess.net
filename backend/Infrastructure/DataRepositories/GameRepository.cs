using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class GameRepository(DomainDataContext context) : Repository<Game>(context), IGameRepository
    {
    }
}
