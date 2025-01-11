using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;
using Domain.Common;

namespace Domain.DTOs
{
    public class GameDetailsDto
    {
        public int GameId { get; set; }
        public string WhitePlayer { get; set; }
        public string BlackPlayer { get; set; }
        public string Winner { get; set; }
        public string Result { get; set; }
        public DateTime Date { get; set; }
        public GameMode GameMode { get; set; }
        public string LastMoveFen { get; set; }
    }

}