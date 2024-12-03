    using Chess.net.Services.Interfaces;
    using Domain.Users;
    using Infrastructure.DataContext;
using Infrastructure.DataRepositories;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

    namespace Chess.net.Services
    {
        public class FriendService(IDataRepository repository, UserManager<User> userManager) : IFriendService
        {
            private readonly UserManager<User> _userManager = userManager;
            private readonly IDataRepository _repository = repository;

            public async Task<bool> AddFriend(string userId, string friendId)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var friend = await _userManager.FindByIdAsync(friendId);

                if (user == null || friend == null || userId == friendId)
                {
                return false;
            }


            var existingFriendship = (await _repository.FriendRepository.GetAllAsync())
                            .FirstOrDefault(f => (f.UserId == userId && f.FriendId == friendId) ||
                                                 (f.UserId == friendId && f.FriendId == userId));

            if (existingFriendship != null)
                {
                    return false;
                }

                var newFriend = new Friend
                {
                    UserId = userId,
                    FriendId = friendId,
                };

               var result =  await _repository.FriendRepository.AddAsync(newFriend);

                return result != -1;
            }
            public async Task<List<User>> ListAllFriends(string userId)
            {
                // Pobierz wszystkie relacje użytkownika
                var friends = (await _repository.FriendRepository.GetAllAsync())
                    .Where(f => f.UserId == userId || f.FriendId == userId)
                    .ToList();

                // Utwórz listę użytkowników na podstawie relacji
                var friendUsers = new List<User>();
                foreach (var friend in friends)
                {
                    var friendId = friend.UserId == userId ? friend.FriendId : friend.UserId;
                    var friendUser = await _userManager.FindByIdAsync(friendId);

                    if (friendUser != null)
                    {
                        friendUsers.Add(friendUser);
                    }
                }

                return friendUsers;
        }


        }
    }
