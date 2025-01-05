namespace Logic.Interfaces
{
    public interface IAdminService
    {
        Task<bool> MakeAdmin(string userId);
        Task<bool> BanUserAndResolveReport(string userId, int reportID);
    }
}
