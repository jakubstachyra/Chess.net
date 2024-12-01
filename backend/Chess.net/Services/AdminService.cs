using Chess.net.Services.Interfaces;
using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Chess.net.Services
{
    public class AdminService(UserManager<User> userManager): IAdminService
    {
        private readonly UserManager<User> _usermanager = userManager;
     
        public async Task<bool> BanUser(string userId)
        {
            var user = await _usermanager.FindByIdAsync(userId);

            if(user == null)
            {
                return false;
            }

            user.IsBanned = true;
            var result = await _usermanager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
