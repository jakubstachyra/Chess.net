namespace Logic.Services.Interfaces
{
    public interface IRankingService
    {
        Task<bool > updateRankingByID(string userID, int pointsDelta);
        Task getUserRankingByID(string userID);
    }
}
