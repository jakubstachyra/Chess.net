using Domain.DTOs;
using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.DataRepositories;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chess.net.Services
{
    public class FriendService(IDataRepository repository, UserManager<User> userManager) : IFriendService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDataRepository _repository = repository;

        public async Task<bool> AddFriend(string userId, string friendName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var friend = await _userManager.FindByNameAsync(friendName);

            if (user == null || friend == null || userId == friend.Id)
            {
                return false;
            }


            var existingFriendship = (await _repository.FriendRepository.GetAllAsync())
                            .FirstOrDefault(f => (f.UserId == userId && f.FriendId == friend.Id) ||
                                                 (f.UserId == friend.Id && f.FriendId == userId));

            if (existingFriendship != null)
            {
                return false;
            }

            var newFriend = new Friend
            {
                UserId = userId,
                FriendId = friend.Id,
            };

            var result = await _repository.FriendRepository.AddAsync(newFriend);

            return result != -1;
        }
        public async Task<List<FriendDto>> ListAllFriends(string userId)
        {
            // Pobierz wszystkie relacje użytkownika
            var friends = await _repository.FriendRepository.GetAllAsync();

            // Filtruj tylko relacje użytkownika
            var friendIds = friends
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                .Distinct()
                .ToList();

            // Utwórz listę DTO
            var friendDtos = new List<FriendDto>();
            foreach (var friendId in friendIds)
            {
                var friendUser = await _userManager.FindByIdAsync(friendId);
                if (friendUser != null)
                {
                    friendDtos.Add(new FriendDto
                    {
                        ID = friendUser.Id,
                        Name = friendUser.UserName!
                    });
                }
            }

            return friendDtos;
        }
        public async Task<bool> RemoveFriend(string userId, string friendId)
        {
            // Tworzenie zapytania przy użyciu metody Query()
            var relation = await _repository.FriendRepository.Query()
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == userId));

            if (relation == null)
            {
                // Relacja nie istnieje
                return false;
            }

            // Usuń relację
            await _repository.FriendRepository.DeleteAsync(relation);

            return true;
        }


    }
}
