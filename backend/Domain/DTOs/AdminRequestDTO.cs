using Domain.Common;
using Domain.Users;

namespace Domain.DTOs
{
    public class AdminRequestDTO
    {
            public required string UserID { get; set; }
            public string? Reason { get; set; }

     }
}
