using Domain.Users;

namespace Logic.Interfaces
{
    public interface IFriendService
    {
        public Task<bool> AddFriend(string userId, string friendId);

        public Task<List<User>> ListAllFriends(string userId);
    }
}
