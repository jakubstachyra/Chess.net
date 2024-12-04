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

        public Move(string v1, string v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public string V1 { get; }
        public string V2 { get; }

        public override string ToString()
        {
            return $"{from} {to}";
        }
    }
}
