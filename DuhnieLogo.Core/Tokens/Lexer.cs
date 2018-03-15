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

                if (character == '~')
                {
                    reader.ReadCharacter();
                }
                else if (character == '+')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Plus));
                }
                else if (character == '-')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Minus));
                }
                else if (character == '*')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Multiply));
                }
                else if (character == '/')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Divide));
                }
                else if (character == ';')
                {
                    while (reader.PeekCharacter() != '\n')
                        reader.ReadCharacter();
                }
                else if (character == '(')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.ParenthesisLeft));
                }
                else if (character == ')')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.ParenthesisRight));
                }
                else if (character == '[')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.BracketLeft));
                }
                else if (character == ']')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.BracketRight));
                }
                else if (character == '"')
                {
                    tokens.Add(ParseStringLiteral(reader));
                }
                else if (character == ':')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Colon));
                }
                else if (character == '=')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Equals));
                }
                else if (character == ',')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Comma));
                }
                else if (Char.IsLetter(character))
                {
                    tokens.Add(ParseKeywordOrIdentifier(reader));
                }
                else if (Char.IsNumber(character))
                {
                    tokens.Add(ParseNumber(reader));
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
            tokens.Add(new Token (TokenType.ProgramEnd, "", reader.CurrentPosition));

            return tokens;
        }

        private static Token ParseStringLiteral(CharacterReader reader)
        {
            reader.ReadCharacter();
            var position = reader.CurrentPosition;

            var buffer = new StringBuilder();

            while(true)
            {
                var character = reader.PeekCharacter().Value;

                if(Char.IsLetterOrDigit(character) || character == '\\')
                {
                    if (Char.IsLetterOrDigit(character))
                        buffer.Append(reader.ReadCharacter());
                    if(character == '\\')
                    {
                        reader.ReadCharacter();
                        buffer.Append(reader.ReadCharacter());
                    }
                }
                else
                {
                    return new Token(TokenType.StringLiteral, buffer.ToString(), position);
                }
            }
        }

        private static Token ReadCharacterAndCreateToken(CharacterReader reader, TokenType type)
        {
            var character = reader.ReadCharacter();
            var position = reader.CurrentPosition;

            return new Token(type, character.Value.ToString(), position);
        }

        private static Token ParseKeywordOrIdentifier(CharacterReader reader)
        {
            var buffer = new StringBuilder();
            TokenPosition position = null;
            while (true)
            {
                var character = reader.PeekCharacter().Value;

                if (Char.IsLetter(character) || (Char.IsNumber(character) && buffer.Length > 0))
                {
                    buffer.Append(character);
                    reader.ReadCharacter();

                    if (position == null)
                        position = reader.CurrentPosition;
                }
                else
                {
                    var value = buffer.ToString().ToLower();

                    if (value == "maak" || value == "naam")
                        return new Token(TokenType.Make, value, position);

                    if (value == "leer")
                        return new Token(TokenType.Learn, value, position);

                    if (value == "eind")
                        return new Token(TokenType.End, value, position);

                    if (value == "uitvoer")
                        return new Token(TokenType.Return, value, position);

                    if (value == "stop")
                        return new Token(TokenType.Stop, value, position);

                    return new Token(TokenType.Identifier, value, position);
                }
            }
        }

        private static Token ParseNumber(CharacterReader reader)
        {
            var buffer = new StringBuilder();
            TokenPosition position = null;

            while (true)
            {
                var character = reader.PeekCharacter().Value;

                if (Char.IsNumber(character))
                {
                    buffer.Append(character);
                    reader.ReadCharacter();

                    if (position == null)
                        position = reader.CurrentPosition;
                }
                else
                {
                    return new Token(TokenType.Integer, buffer.ToString(), position);
                }
            }
        }
    }
}
