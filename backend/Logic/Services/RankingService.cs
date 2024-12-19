using Domain.Users;
using Logic.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Logic.Services
{
    public class RankingService(UserManager<User> userManager): IRankingService
    {
        private UserManager<User> _userManager = userManager;

        public async Task<bool> updateRanking(string userID, int pointsDelta)
        {
            var user = await _userManager.FindByEmailAsync(userID);

            if(user == null)
            {
                return false;
            }
            
            user.
        }
    }
}
