using Domain.DTOs;

namespace Logic.Interfaces
{
    public interface IFriendService
    {
        public Task<bool> AddFriend(string userId, string friendId);

        public Task<List<FriendDto>> ListAllFriends(string userId);
        public Task<bool> RemoveFriend(string userId, string friendId);
    }
}
