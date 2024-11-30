using System.ComponentModel.DataAnnotations;


namespace Domain.Common
{
    public class Ranking: Base
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
