using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain.User
{
    public class User: IdentityUser
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public bool IsBanned { get; set; }
    }
}
