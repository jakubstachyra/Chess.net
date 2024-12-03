using Microsoft.AspNetCore.Mvc;

namespace Chess.net.Services.Interfaces
{
    public interface IAdminService
    {
        public Task<bool> BanUser(string userId);
    }
}
