using Domain.Users;

namespace Chess.net.Services.Interfaces
{
    public interface IFriendService
    {
        public Task<bool> AddFriend(int userId, int friendId);

        public Task<List<User>> ListAllFriends(int userId);
    }
}
