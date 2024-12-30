using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Chess.net.Services
{
    public class AdminService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager): IAdminService
    {
        private readonly UserManager<User> _usermanager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<bool> BanUser(string userId)

        {
            var user = await _usermanager.FindByIdAsync(userId);

            if(user == null)
            {
                return false;
            }
            if(user.IsBanned == true)
            {
                return true;
            }

            user.IsBanned = true;
            var result = await _usermanager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> MakeAdmin(string userId)
        {
            var user = await _usermanager.FindByIdAsync(userId);

            if (user == null)
            {
                return false; 
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded)
                {
                    return false; 
                }
            }

            var result = await _usermanager.AddToRoleAsync(user, "Admin");

            return result.Succeeded;
        }

    }
}
