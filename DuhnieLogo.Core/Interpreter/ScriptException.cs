using DuhnieLogo.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    public class ScriptException : Exception
    {
        public Token Token { get; }

        public ScriptException(string message) : base(message)
        { }

        public ScriptException(string message, Token token) : base(message)
        {
            Token = token;
        }

        public ScriptException(string message, Token token, Exception innerException) : base(message,innerException)
        {
            Token = token;
        }
    }
}
