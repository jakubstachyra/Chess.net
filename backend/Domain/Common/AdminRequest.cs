using Domain.Users;

namespace Domain.Common
{
    public class AdminRequest : Base
    {
        public required string UserID { get; set; } 
        public string? Reason { get; set; } 
        public DateTime RequestDate { get; set; }
        public bool isResolved { get; set; } = false; 
    }

}
