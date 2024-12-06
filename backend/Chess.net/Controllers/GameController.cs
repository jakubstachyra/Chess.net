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

        public List<string> Get(int gameId)
        {
            Console.WriteLine("controler");
            Console.WriteLine(gameId);
            List<string> a = new List<string>();
            return _gameService.GetAllPlayerMoves(gameId).Select(move => move.ToString()).ToList();
            Console.Clear();
        }

        [HttpPost("ReceiveMove")]

        public void Post([FromBody] string move, [FromRoute] int gameId)
        {

            Console.WriteLine(move);
            Console.WriteLine(gameId);
            _gameService.MakeSentMove(gameId,move);
        }

        //[HttpGet("getBlackMove")]

        //public string SendBlackMove()
        //{
        //    return _gameService.CalculateBlackMove().ToString();
        //}

        //[HttpGet("Fen")]
        //public string SendFen()
        //{
        //    return _gameService.SendFen();
        //}

        //[HttpGet("WhoToMove")]

        //public string WhoToMove()
        //{
        //    return _gameService.WhoToMove();
        //}
        //[HttpGet("check-claims")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public IActionResult CheckClaims()
        //{
        //    var claims = User.Claims.Select(c => new { c.Type, c.Value });
        //    return Ok(claims);
        //}

        }
}
