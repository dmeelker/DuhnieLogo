using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Tokens
{
    public static class Lexer
    {
        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var reader = new CharacterReader(input + "\n");
            var buffer = new StringBuilder();

            while (!reader.AtEnd)
            {
                char character = reader.PeekCharacter().Value;

                if (character == '+')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.Plus, Value = "+"});
                }
                else if (character == '-')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.Minus, Value = "-" });
                }
                else if (character == '*')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.Multiply, Value = "*" });
                }
                else if (character == '/')
                {
                    reader.ReadCharacter();

                    if(reader.PeekCharacter() == '/')
                    {
                        while (reader.PeekCharacter() != '\n')
                            reader.ReadCharacter();
                    }
                    else
                        tokens.Add(new Token() { Type = TokenType.Divide, Value = "/" });
                }
                else if (character == '(')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.ParenthesisLeft, Value = "(" });
                }
                else if (character == ')')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.ParenthesisRight, Value = ")" });
                }
                else if (character == '"')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.DoubleQuote, Value = "\"" });
                }
                else if (character == ':')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.Colon, Value = ":" });
                }
                else if (character == ';')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.Semicolon, Value = ";" });
                }
                else if (character == '=')
                {
                    reader.ReadCharacter();

                    if(reader.PeekCharacter() == '=')
                    {
                        reader.ReadCharacter();
                        tokens.Add(new Token() { Type = TokenType.Equals, Value = "==" });
                    }
                    else
                        tokens.Add(new Token() { Type = TokenType.Assign, Value = "=" });
                }
                else if (character == '&')
                {
                    reader.ReadCharacter();

                    if (reader.PeekCharacter() == '&')
                    {
                        reader.ReadCharacter();
                        tokens.Add(new Token() { Type = TokenType.LogicalAnd, Value = "&&" });
                    }
                    else
                        throw new Exception();
                }
                else if (character == '|')
                {
                    reader.ReadCharacter();

                    if (reader.PeekCharacter() == '|')
                    {
                        reader.ReadCharacter();
                        tokens.Add(new Token() { Type = TokenType.LogicalOr, Value = "||" });
                    }
                    else
                        throw new Exception();
                }
                else if (character == ',')
                {
                    reader.ReadCharacter();
                    tokens.Add(new Token() { Type = TokenType.Comma, Value = "," });
                }
                else if (Char.IsLetter(character))
                {
                    tokens.Add(ParseKeywordOrIdentifier(reader));
                }
                else if (Char.IsNumber(character))
                {
                    tokens.Add(ParseNumber(reader));
                }
                else if (character == '"')
                {
                    tokens.Add(ParseStringLiteral(reader));
                }
                else if (Char.IsWhiteSpace(character) || character == '\n' || character == '\r')
                {
                    // Skip
                    reader.ReadCharacter();
                }
                else
                {
                    throw new Exception($"Unexpected character: {character}");
                }
            }
            tokens.Add(new Token { Type = TokenType.ProgramEnd });

            return tokens;
        }

        private static Token ParseKeywordOrIdentifier(CharacterReader reader)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                var character = reader.PeekCharacter().Value;

                if (Char.IsLetter(character) || (Char.IsNumber(character) && buffer.Length > 0))
                {
                    buffer.Append(character);
                    reader.ReadCharacter();
                }
                else
                {
                    var value = buffer.ToString();

                    
                    if (value == "maak" || value == "naam")
                        return new Token() { Type = TokenType.Make, Value = value };

                    if (value == "leer")
                        return new Token() { Type = TokenType.Learn, Value = value };

                    if (value == "eind")
                        return new Token() { Type = TokenType.End, Value = value };

                    return new Token()
                    {
                        Type = TokenType.Word,
                        Value = value
                    };
                }
            }
        }

        private static Token ParseNumber(CharacterReader reader)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                var character = reader.PeekCharacter().Value;

                if (Char.IsNumber(character))
                {
                    buffer.Append(character);
                    reader.ReadCharacter();
                }
                else
                {
                    return new Token()
                    {
                        Type = TokenType.Integer,
                        Value = buffer.ToString()
                    };
                }
            }
        }

        private static Token ParseStringLiteral(CharacterReader reader)
        {
            var buffer = new StringBuilder();

            reader.ReadCharacter(); // Read the opening character

            while (true)
            {
                var character = reader.PeekCharacter().Value;

                if (character == '"')
                {
                    reader.ReadCharacter();

                    return new Token()
                    {
                        Type = TokenType.StringLiteral,
                        Value = buffer.ToString()
                    };
                }
                else
                {
                    buffer.Append(character);
                    reader.ReadCharacter();
                }
            }
        }
    }
}
