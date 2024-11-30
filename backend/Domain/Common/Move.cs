using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public class Move: Base
    {
        [Required]
        public int GameID { get; set; }
        [Required]
        public required Game Game { get; set; }
        [Required]
        public int MoveNumber { get; set; }
        //Co jeśli partia konczy sie z jednym rucham białego, bez ruchu czarnego?
        public string WhiteMove {  get; set; } = string.Empty;
        public string BlackMove { get; set; } = string.Empty;
        public long WhiteRemainingTimeMs { get; set; }
        public long BlackRemainingTimeMs { get; set;}
    }
}
