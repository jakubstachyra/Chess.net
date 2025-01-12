using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Utils
{
    public class Converter
    {
        public static Position? ChessNotationToPosition(string ChessNotation)
        {
            ChessNotation = ChessNotation.ToLower();
            return new Position(ChessNotation[0] - 'a', ChessNotation[1] - '1');
        }

        public static string ChessPositionToString(Position position)
        {
            return $"{(char)('A' + position.x)}{position.y + 1}";
        }
    }
}
