using Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chess.net.Controllers
{
    [ApiController]
    public class FriendController(IFriendService friendService) : Controller
    {
        private readonly IFriendService _friendService = friendService;


        [HttpPost("friends/{userID}")]
        public async Task<IActionResult> AddFriend(string userID, [FromBody] string friendName)
        {
            var result = await _friendService.AddFriend(userID, friendName);
            
            if(!result)
            {
                return BadRequest(result);
            }

            return Ok();
        }
        [HttpGet("friends/{userID}/")]
        public async Task<IActionResult> GetAllUserFriends(string userID)
        {
            var result = await _friendService.ListAllFriends(userID);
            return Ok(result);
        }
        [HttpDelete("friends/{userID}")]
        public async Task<IActionResult> RemoveFriend(string userID, [FromBody] string friendID)
        {
            var result = await _friendService.RemoveFriend(userID, friendID);
            if (!result)
            {
                return BadRequest("Failed to remove friend. The friendship may not exist.");
            }

            return Ok("Friend removed successfully.");
        }

    }
}
