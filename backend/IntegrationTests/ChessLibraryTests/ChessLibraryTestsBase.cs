using Logic.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessGame;
using ChessGame.GameMechanics;

namespace IntegrationTests.ChessLibraryTests
{
    [TestFixture]
    public class ChessLibraryTestsBase
    {
        protected int gameId;
        protected Game game;
        [SetUp]
        public void SetUp()
        {
            gameId = 1;
            game = new Game(gameId);
        }

    }
}
