using Chess.net.Services;
using Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using NuGet.Protocol;

namespace Chess.net.Controllers
{
    [ApiController]
    public class HistoryController(IHistoryService historyService) : ControllerBase
    {
        private readonly IHistoryService _historyService = historyService;

        [HttpGet("/getGameHistoryByID/{gameID}")]
        public async Task<IActionResult> GetGameHistoryByID(int gameID)
        {
           var moves = await _historyService.GetGameHistoryByGameID(gameID, true);

            if(moves == null) { return NotFound(); }

            return Ok(moves.ToJson());
        }

        [HttpGet("games/{userID}")]
        public async Task<IActionResult> GetPlayerGames(
        string userID,
        [FromQuery] int limit = 6,
        [FromQuery] int offset = 0,
        [FromQuery] bool detailed = false)
        {
            if (string.IsNullOrEmpty(userID))
            {
                return BadRequest("Player ID is required.");
            }

            if (detailed)
            {
                // Pobierz pełne dane dla każdej gry
                var games = await _historyService.GetRecentGamesByPlayerId(userID, limit, offset);
                return Ok(games);
            }
            else
            {
                // Pobierz podsumowanie gier
                var summaries = await _historyService.GetRecentGamesByPlayerId(userID, limit, offset);
                return Ok(summaries);
            }
        }
    }
}
