using Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;

namespace Chess.net.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RankingController(IRankingService rankingService) : Controller
    {
        private readonly IRankingService _rankingService = rankingService;

        [HttpGet("/getUserRankingsByUserID/{userID}")]
        public async Task<IActionResult> getAllUserRankings(string userID)
        {
            var result = await _rankingService.getUserRankingsByID(userID);
        
            if(!result.Any()) { return NotFound("Rankings have not been found."); }

            return Ok(result.ToList().ToJson());

        }
    }
}
