using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;

namespace Infrastructure.DataRepositories
{
    public class FriendRepository(DomainDataContext context): Repository<Friend>(context), IFriendRepository
    {
    }
}
