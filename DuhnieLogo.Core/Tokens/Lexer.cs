﻿using System;
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
                    reader.ReadCharacter();
                    var position = reader.CurrentPosition;

                    if (reader.PeekCharacter() == '/')
                    {
                        while (reader.PeekCharacter() != '\n')
                            reader.ReadCharacter();
                    }
                    else
                        tokens.Add(new Token(TokenType.Divide, "/", position));
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
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.DoubleQuote));
                }
                else if (character == ':')
                {
                    tokens.Add(ReadCharacterAndCreateToken(reader, TokenType.Colon));
                }
                else if (character == '=')
                {
                    reader.ReadCharacter();
                    var position = reader.CurrentPosition;

                    if(reader.PeekCharacter() == '=')
                    {
                        reader.ReadCharacter();
                        tokens.Add(new Token(TokenType.Equals, "==", position));
                    }
                    else
                        tokens.Add(new Token(TokenType.Assign, "=", position));
                }
                else if (character == '&')
                {
                    reader.ReadCharacter();
                    var position = reader.CurrentPosition;

                    if (reader.PeekCharacter() == '&')
                    {
                        reader.ReadCharacter();
                        tokens.Add(new Token(TokenType.LogicalAnd, "&&", position));
                    }
                    else
                        throw new Exception();
                }
                else if (character == '|')
                {
                    reader.ReadCharacter();
                    var position = reader.CurrentPosition;

                    if (reader.PeekCharacter() == '|')
                    {
                        reader.ReadCharacter();
                        tokens.Add(new Token(TokenType.LogicalOr, "||", position));
                    }
                    else
                        throw new Exception();
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
                    var value = buffer.ToString();

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

                    return new Token(TokenType.Word, value, position);
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
