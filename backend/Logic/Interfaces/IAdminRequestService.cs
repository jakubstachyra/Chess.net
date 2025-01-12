using Domain.Common;
using Domain.DTOs;

namespace Logic.Interfaces
{
    public interface IAdminRequestService
    {
        Task<List<AdminRequest>> GetPendingRequestsAsync();
        Task<bool> CreateRequestAsync(AdminRequestDTO request);
        Task<bool> UpdateRequestStatusAsync(int id);
    }
}
