using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class DataRepository(DomainDataContext context): IDataRepository
    {
        public IGameModeRepository GameModeRepository { get; set; } = new GameModeRepository(context);
        public IGameRepository GameRepository { get; set; } = new GameRepository(context);
        public IMoveRepository MoveRepository { get; set; } = new MoveRepository(context);
        public IRankingRepository RankingRepository { get; set; } = new RankingRepository(context);
        public IRankingsUserRepository RankingsUserRepository { get; set;} = new RankingsUserRepository(context);
    }
}
