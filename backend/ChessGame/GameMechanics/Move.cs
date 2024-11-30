using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.GameMechanics
{
    public class Move
    {
        public Position from;
        public Position to;
        public Move(Position from, Position to)
        {
            this.from = from;
            this.to = to;
        }
        public override string ToString()
        {
            return $"{from} {to}";
        }
    }
}
