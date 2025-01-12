using Domain.Users;
using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public class Game : Base
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

        // Nowe pole do przechowywania pozycji początkowej
        [Required]
        [StringLength(100)]
        public string StartFen { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // Klasyczna pozycja startowa
    }
}
