using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Tokens
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }

    public enum TokenType
    {
        Identifier,
        Word,
        Integer,
        StringLiteral,
        Comma,
        DoubleQuote,

        Var,
        If,
        Else,

        LogicalAnd,
        LogicalOr,
        Equals,
        Plus,
        Minus,
        Multiply,
        Divide,

        ParenthesisLeft,
        ParenthesisRight,

        Semicolon,
        Assign,
        Begin,
        End,

        Procedure,
        Return,
        ProgramEnd,

        NewLine,

        Make
    }
}
