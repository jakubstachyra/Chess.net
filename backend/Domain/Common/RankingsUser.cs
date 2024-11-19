using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class RankingsUser : Base
    {
        [Required]
        public int UserID {get; set;}
        [Required]
        public int RankingID {get; set;}
        [Required]
        public int Points { get; set;}
        [Required]
        public int Position { get; set;}
    }
}
