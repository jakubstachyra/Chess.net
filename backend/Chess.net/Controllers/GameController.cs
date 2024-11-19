using Chess.net.Services.Interfaces;
using ChessGame;
using ChessGame.GameMechanics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chess.net.Controllers
{
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        public GameController(IGameService gameService)
        {
            _gameService = gameService;
            _gameService.InitializeGame(1);
        }

        [HttpGet("moves")]  
        public List<string> Get()
        {
            Console.WriteLine("controler");
            List<string> a = new List<string>();
            return _gameService.GetAllPlayerMoves().Select(move=>move.ToString()).ToList();
            Console.Clear();
        }

        [HttpPost("ReceiveMove")]

        public void Post([FromBody] string move)
        {
            
            Console.WriteLine(move);
            _gameService.MakeSentMove(move);
        }

        [HttpGet("getBlackMove")]

        public string SendBlackMove()
        {
            return _gameService.CalculateBlackMove().ToString();
        }

        [HttpGet("Fen")]
        public string SendFen()
        {
            return _gameService.SendFen();
        }

        [HttpGet("WhoToMove")]

        public string WhoToMove()
        {
            return _gameService.WhoToMove();
        }
    }
}
