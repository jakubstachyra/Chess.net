using System.ComponentModel.DataAnnotations;
using Domain.Users;

namespace Domain.Common
{
    public class RankingsUser : Base
    {
        [Required]
        public int UserID {get; set;}

        [Required]
        public required User User { get; set;}

        [Required]
        public int RankingID {get; set;}

        [Required]

        public required Ranking Ranking { get; set;}
      
        public int Points { get; set;}

        [Required]
        public int Position { get; set;}
    }
}
