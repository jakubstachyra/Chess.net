using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Domain.Users
{
    public class User: IdentityUser
    {
        [Required]
        public bool IsBanned { get; set; } = false;
        [Required]
        public bool IsVerified { get; set; } = false;

    }
}
