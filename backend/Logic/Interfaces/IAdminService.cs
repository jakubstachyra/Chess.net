namespace Logic.Interfaces
{
    public interface IAdminService
    {
        Task<bool> MakeAdmin(int requestID, string userId);
        Task<bool> BanUserAndResolveReport(string userId, int reportID);
    }
}
