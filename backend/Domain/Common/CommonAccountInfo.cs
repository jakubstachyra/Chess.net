using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class CommonAccountInfo: Base
    {
        [Required(ErrorMessage = ("userid is required"))]
        [ForeignKey("userID")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "username is required" )]
        [MaxLength(80)]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "email is required")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
    }
}
