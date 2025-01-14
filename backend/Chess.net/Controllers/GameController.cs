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
        //[HttpPost("createGame")]
        //public IActionResult CreateGame()
        //{
        //    var gameId = _gameService.InitializeGame();
        //    return Ok(new { id = gameId });
        //}


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

        public  async Task<string> SendBlackMove([FromRoute] int gameId)
        {
            var result =  _gameService.CalculateComputerMove(gameId);

            return result.ToString();
        }

        [HttpGet("Fen/{gameId}")]
        public string SendFen([FromRoute] int gameId)
        {
            return _gameService.SendFen(gameId);
        }

        [HttpPost("GetFen/{gameId}")]
        public void ReceiveFen([FromRoute] int gameId, [FromBody] string FEN)
        {
           _gameService.ReceiveFen(gameId, FEN);
        }

        [HttpGet("WhoToMove/{gameId}")]

        public string WhoToMove([FromRoute] int gameId)
        {
            return _gameService.WhoToMove(gameId).ToString();
        }


        [HttpGet("State/{gameId}")]

        public  async Task<bool> GameState([FromRoute]int gameId)
        {
            return await _gameService.GetGameState(gameId);
        }

        [HttpPost("InitializeWithComputer")]
        [AllowAnonymous]
        public IActionResult InitializeGameWithComputer()
        {
            var userId = User.Identity?.IsAuthenticated == true
                ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                : "guest";

            int gameId = _gameService.InitializeGameWithComputer(userId);
            return Ok(new { GameId = gameId });
        }
        [HttpPost("resign/{gameId}")]
        public async Task<IActionResult> Resign([FromRoute] int gameId)
        {
            // If you're using JWT or Identity for auth, you can get userId from the claims:
            var userId = User.Identity?.IsAuthenticated == true
                ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                : "guest";

            try
            {
                await _gameService.ResignGame(gameId, userId);
                return Ok("You have resigned from the game.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

