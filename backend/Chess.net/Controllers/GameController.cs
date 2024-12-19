using Chess.net.Services;
using Chess.net.Services.Interfaces;
using ChessGame;
using ChessGame.GameMechanics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chess.net.Controllers
{
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }
        [HttpPost("createGame")]
        public string CreateGame()
        {
                return _gameService.InitializeGame().ToString();

        }

        [HttpGet("moves/{gameId}")]

        public List<string> GetMoves([FromRoute] int gameId)
        {
            List<string> a = new List<string>();
            var b = _gameService.GetAllPlayerMoves(gameId);
            foreach (var move in b)
            {
                a.Add(move.ToString());
            }
            return a;
            
        }

        [HttpPost("ReceiveMove/{gameId}")]

        public void Post([FromBody] string move, [FromRoute] int gameId)
        {
            _gameService.MakeSentMove(gameId,move);
        }

        [HttpGet("getComputerMove/{gameId}")]

        public string SendBlackMove([FromRoute] int gameId)
        {
            return _gameService.CalculateComputerMove(gameId).ToString();
        }

        [HttpGet("Fen/{gameId}")]
        public string SendFen([FromRoute] int gameId)
        {
            return _gameService.SendFen(gameId);
        }

        [HttpGet("WhoToMove/{gameId}")]

        public string WhoToMove([FromRoute] int gameId)
        {
            return _gameService.WhoToMove(gameId).ToString();
        }
        [HttpGet("check-claims")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CheckClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

    }
}
