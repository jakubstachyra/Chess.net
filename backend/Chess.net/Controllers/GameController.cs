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
        public int CreateGame()
        {
                return _gameService.InitializeGame();

        }

        [HttpGet("moves/{gameId}")]

        public List<Move> GetMoves([FromRoute] int gameId)
        {
            return _gameService.GetAllPlayerMoves(gameId);
        }

        [HttpPost("ReceiveMove/{gameId}")]

        public void Post([FromBody] string move, [FromRoute] int gameId)
        {
            _gameService.MakeSentMove(gameId,move);
        }

        [HttpGet("getComputerMove/{gameId}")]

        public Move SendBlackMove([FromRoute] int gameId)
        {
            return _gameService.CalculateComputerMove(gameId);
        }

        [HttpGet("Fen/{gameId}")]
        public string SendFen([FromRoute] int gameId)
        {
            return _gameService.SendFen(gameId);
        }

        [HttpGet("WhoToMove/{gameId}")]

        public int WhoToMove([FromRoute] int gameId)
        {
            return _gameService.WhoToMove(gameId);
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
