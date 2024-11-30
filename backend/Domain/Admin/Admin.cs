using Domain.Users;
using System.ComponentModel.DataAnnotations;


namespace Domain.Admin
{
    public class Admin: User
    {
        [Required]
        public bool IsVerified { get; set; }

    }
}
