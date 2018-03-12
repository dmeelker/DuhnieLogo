using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Tokens
{
    class TokenStream
    {
        private readonly Token[] tokens;
        private int currentIndex = 0;

        public TokenStream(Token[] tokens)
        {
            this.tokens = tokens;
        }

        public Token Peek()
        {
            if (!CanPeek)
                return null;

            return tokens[currentIndex + 1];
        }

        //public void Read()
        //{
        //    if (!CanRead)
        //        throw new Exception("End of token stream reached");

        //    currentIndex++;
        //    return tokens[currentIndex];
        //}

        public Token CurrentToken => currentIndex < tokens.Length ? tokens[currentIndex] : null;

        public Token Eat(TokenType type)
        {
            var token = CurrentToken;
            if (token.Type != type)
                throw new Exception($"Unexpected token: {token}, expected {type}");

            currentIndex++;
            return token;
        }

        public bool CanRead => currentIndex < tokens.Length - 1;

        public bool CanPeek => currentIndex < tokens.Length - 2;

        public int Location => currentIndex;

        public void Seek(int location)
        {
            if (location < 0 || location >= tokens.Length)
                throw new Exception($"Illegal location: {location}");

            currentIndex = location;
        }
    }
}
