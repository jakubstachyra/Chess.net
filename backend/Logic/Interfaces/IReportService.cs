using Domain.Common;

namespace Logic.Interfaces
{
    public interface IReportService
    {
        Task<bool> ReportUserAsync(string userID, int gameID);
        Task<IEnumerable<Report>> GetAllActiveReports();
    }
}
