﻿using System;
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
        public string LiteralValue { get; set; }
        public TokenPosition Location { get; set; }

        public Token(TokenType type, string value, TokenPosition location) : this(type, value, value, location)
        {
        }

        public Token(TokenType type, string value, string literalValue, TokenPosition location)
        {
            Type = type;
            Value = value;
            LiteralValue = literalValue;
            Location = location;
        }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }

    public enum TokenType
    {
        Identifier,
        Integer,
        StringLiteral,
        Comma,

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
        BracketLeft,
        BracketRight,

        Colon,
        Semicolon,
        Assign,

        Learn,
        End,
        Return,
        Stop,
        ProgramEnd,

        NewLine,

        Make
    }
}
