using Chess.net.Services;
using ChessGame;
using ChessGame.GameMechanics;
using Domain.DTOs;
using Logic.Interfaces;
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
        public IActionResult CreateGame()
        {
            var gameId = _gameService.InitializeGame();
            return Ok(new { id = gameId });
        }


        [HttpGet("moves/{gameId}")]

        public List<string> GetMoves([FromRoute] int gameId)
        {
 return _gameService.GetAllPlayerMoves(gameId).Select(move=>move.ToString()).ToList();

        }

        [HttpPost("ReceiveMove/{gameId}")]
        public IActionResult Post([FromBody] MoveDto moveDto, [FromRoute] int gameId)
        {
            if (string.IsNullOrEmpty(moveDto?.Move))
            {
                return BadRequest("Move is required.");
            }

            _gameService.MakeSentMove(gameId, moveDto.Move);

            return Ok("Move received.");
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
