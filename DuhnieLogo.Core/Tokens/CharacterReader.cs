using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Tokens
{
    class CharacterReader
    {
        private string input;
        private int currentIndex;
        public int CurrentRow { get; private set; } = 0;
        public int CurrentColumn { get; private set; } = 0;
        
        public TokenPosition CurrentPosition => new TokenPosition(CurrentRow, CurrentColumn, currentIndex);

        public CharacterReader(string input)
        {
            this.input = input;
            currentIndex = -1;
        }

        public char? ReadCharacter()
        {
            if (AtEnd)
                return null;

            if (currentIndex > -1)
            {
                if (input[currentIndex] == '\n')
                {
                    CurrentRow++;
                    CurrentColumn = 0;
                }
                else
                {
                    CurrentColumn++;
                }
            }

            currentIndex++;
            return input[currentIndex];
        }

        public char? PeekCharacter()
        {
            if (!CanPeek)
                return null;

            return input[currentIndex + 1];
        }

        public bool AtEnd => currentIndex == input.Length - 1;
        public bool CanPeek => currentIndex < input.Length - 1;
    }
}
