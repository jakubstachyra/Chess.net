using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.GameMechanics
{
    public class Move
    {
        public Position from { get; set; }
        public Position to { get; set; }
        public string V1 { get; }
        public string V2 { get; }
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



        public override string ToString()
        {
            return $"{from} {to}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Move other)
            {
                return (from.x==other.from.x && to.x==other.to.x && from.y == other.from.y && to.y == other.to.y);
            }
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
