using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class GameModeRepository(DomainDataContext context): Repository<GameMode>(context), IGameModeRepository
    {
    }
}
