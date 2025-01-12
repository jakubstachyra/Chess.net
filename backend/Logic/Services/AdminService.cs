using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Chess.net.Services
{
    public class AdminService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
        DomainDataContext domainDataContext, IReportService reportService,
        IAdminRequestService adminRequestService) : IAdminService
    {
        private readonly UserManager<User> _usermanager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly DomainDataContext _dataContext = domainDataContext;
        private readonly IReportService _reportService = reportService;
        private readonly IAdminRequestService _adminRequestService = adminRequestService;

        public async Task<bool> BanUserAndResolveReport(string userId, int reportID)
        {
            using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _usermanager.FindByIdAsync(userId)
                           ?? throw new Exception("User not found");

                if (user.IsBanned)
                {
                    throw new InvalidOperationException("User is already banned.");
                }

                user.IsBanned = true;
                var result = await _usermanager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to update user ban status.");
                }

                var reportResult = await _reportService.MakeReportResolved(reportID);
                if (!reportResult)
                {
                    throw new Exception("Failed to resolve report.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> MakeAdmin(int requestID, string userId)
        {
            using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _usermanager.FindByIdAsync(userId)
                           ?? throw new Exception("User not found");

                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception("Failed to create 'Admin' role.");
                    }
                }

                var isRequestResolved = await _adminRequestService.UpdateRequestStatusAsync(requestID);
                if (!isRequestResolved)
                {
                    throw new Exception("Failed to resolve admin request.");
                }

                var result = await _usermanager.AddToRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to add user to 'Admin' role.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
