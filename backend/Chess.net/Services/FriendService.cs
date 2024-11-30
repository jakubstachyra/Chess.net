﻿    using Chess.net.Services.Interfaces;
    using Domain.Users;
    using Infrastructure.DataContext;
    using Microsoft.EntityFrameworkCore;

    namespace Chess.net.Services
    {
        public class FriendService : IFriendService
        {
            private readonly DomainDataContext _context;

            public FriendService(DomainDataContext context)
            {
                _context = context;
            }

            public async Task<bool> AddFriend(string userId, string friendId)
            {
                var user = await _context.Users.FindAsync(userId);
                var friend = await _context.Users.FindAsync(friendId);

                if (user == null || friend == null || userId == friendId)
                {
                    return false; 
                }

            
                var existingFriendship = await _context.Friends
                    .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId) ||
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


                _context.Friends.Add(newFriend);

                await _context.SaveChangesAsync();
                return true;
            }
            public async Task<List<User>> ListAllFriends(string userId)
            {
                return await _context.Friends
                    .Where(f => (f.UserId == userId || f.FriendId == userId) && f.UserId != f.FriendId)
                    .Select(f => f.UserId == userId ? f.FriendUser : f.User)  
                    .ToListAsync();
            }


        }
    }