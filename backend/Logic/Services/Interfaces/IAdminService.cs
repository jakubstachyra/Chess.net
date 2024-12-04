
namespace Chess.net.Services.Interfaces
{
    public interface IAdminService
    {
        Task<bool> BanUser(string userId);
        Task<bool> MakeAdmin(string userId);
    }
}
