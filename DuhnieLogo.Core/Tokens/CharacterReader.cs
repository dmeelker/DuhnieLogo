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

        public CharacterReader(string input)
        {
            this.input = input;
            currentIndex = -1;
        }

        public char? ReadCharacter()
        {
            if (AtEnd)
                return null;

            currentIndex++;
            var chr = input[currentIndex];
            

            return chr;
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
