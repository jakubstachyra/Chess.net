namespace Logic.Interfaces
{
    public interface IAdminService
    {
        Task<bool> BanUser(string userId);
        Task<bool> MakeAdmin(string userId);
    }
}
