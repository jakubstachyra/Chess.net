using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Chess.net.Services
{
    public class AdminService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
        DomainDataContext domainDataContext, IReportService reportService): IAdminService
    {
        private readonly UserManager<User> _usermanager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly DomainDataContext _dataContext = domainDataContext;
        private readonly IReportService _reportService = reportService;
        public async Task<bool> BanUserAndResolveReport(string userId, int reportID)
        {
            using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _usermanager.FindByIdAsync(userId);

                if (user == null)
                {
                    return false;
                }
                if (user.IsBanned == true)
                {
                    return true;
                }

                user.IsBanned = true;
                var result = await _usermanager.UpdateAsync(user);
                
                var reportResult = await _reportService.MakeReportResolved(reportID);
                if(!reportResult)
                {
                    return false;
                }

                await transaction.CommitAsync();
                return result.Succeeded;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
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
