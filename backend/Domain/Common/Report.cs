using Domain.Users;

namespace Domain.Common
{
    public class Report: Base
    {
        public required string SuspectID {  get; set; }
        public required User  Suspect { get; set; }
        public required int GameID { get; set; }
        public required Game Game { get; set; }
        public bool isResolved { get; set; } = false;
    }
}
