using Domain.Common;

namespace Logic.Interfaces
{
    public interface IHistoryService
    {
        Task<Object> GetGameHistoryByGameID(int ID);
    }
}
