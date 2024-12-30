using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IDataRepository
    {
        public IGameModeRepository GameModeRepository { get; set; }
        public IGameRepository GameRepository { get; set; }
        public IMoveRepository MoveRepository { get; set; }
        public IRankingRepository RankingRepository { get; set; }
        public IRankingsUserRepository RankingsUserRepository { get; set;}
        public IFriendRepository FriendRepository { get; set; }
        public IReportsRepository ReportsRepository { get; set; }
        public Task<IDbContextTransaction> BeginTransactionAsync();

    }
}
