using Domain.Common;
using System.ComponentModel.DataAnnotations;


namespace Domain.Admin
{
    public class Admin: Base
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public bool IsVerified { get; set; }
    }
}
