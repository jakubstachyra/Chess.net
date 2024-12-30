using Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chess.net.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController(IAdminService adminService) : ControllerBase
    {
        private readonly IAdminService _adminService = adminService;

        [HttpPatch("/ban/{userId}")]
        public  async Task<IActionResult> BanUser(string userId)
        {
            var success = await _adminService.BanUser(userId);

            if (!success) { return NotFound(new { Message = "User not found." }); }

            return Ok(new { Message = "User has been banned successfully." });
        }
        [HttpPatch("/make/{userid}")]
        public async Task<IActionResult> MakeAdmin(string userid)
        {
            var success = await _adminService.MakeAdmin(userid);
            if(!success) { return NotFound(new { Message = "User not maked admin" }); }

            return Ok(new { Message = "User has been made admin" });
        }
    }
}
