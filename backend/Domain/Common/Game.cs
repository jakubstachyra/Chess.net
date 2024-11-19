using Domain.Users;
using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public class Game: Base
    {
        [Required]
        public User WhitePlayer { get; set; }
        [Required]
        public User BlackPlayer { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        public string Result { get; set; }
    }
}
