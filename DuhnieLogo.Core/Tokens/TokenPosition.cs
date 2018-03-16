using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Tokens
{
    public class TokenPosition
    {
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Character { get; private set; }

        public TokenPosition(int row, int column, int character)
        {
            Row = row;
            Column = column;
            Character = character;
        }
    }
}
