using Chess.net.Services;
using Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using NuGet.Protocol;

namespace Chess.net.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HistoryController(IHistoryService historyService) : ControllerBase
    {
        private readonly IHistoryService _historyService = historyService;

        [HttpGet("/getGameHistoryByID/{gameID}")]
        public async Task<IActionResult> GetGameHistoryByID(int gameID)
        {
           var moves = await _historyService.GetGameHistoryByGameID(gameID);

            if(moves == null) { return NotFound(); }

            return Ok(moves.ToJson());
        }
    }
}
