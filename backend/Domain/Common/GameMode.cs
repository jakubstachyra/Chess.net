using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class GameMode: Base
    {
        [Required]
        public required string Description { get; set; }
    }
}
