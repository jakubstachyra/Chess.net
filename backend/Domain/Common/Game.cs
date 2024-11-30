using Domain.Users;
using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public class Game: Base
    {
        [Required]
        public required User WhitePlayer { get; set; }
        [Required]
        public required User BlackPlayer { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        [Required]
        public string Result { get; set; } = "0-0";

        [Required]

        public required GameMode GameMode { get; set; }
    }
}
