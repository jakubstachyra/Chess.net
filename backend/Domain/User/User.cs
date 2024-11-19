using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
