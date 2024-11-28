using System.ComponentModel.DataAnnotations;

namespace Domain.Users
{
    public class Friend
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  

        public User User { get; set; }   

        [Required]
        public int FriendId { get; set; }  

        public User FriendUser { get; set; } 
    }
}
