using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain.Users
{
    public class User: IdentityUser
    {
        [Required]
        public bool IsBanned { get; set; } = false;
    }
}
