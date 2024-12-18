﻿using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Users
{
    public class Friend:Base
    {
        [Required]
        public string UserId { get; set; } 

        [ForeignKey("UserId")]
        public User User { get; set; }    

        [Required]
        public string FriendId { get; set; }  

        [ForeignKey("FriendId")]
        public User FriendUser { get; set; } 
    }
}
