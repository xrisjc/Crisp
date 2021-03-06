﻿using System.Collections.Generic;

namespace Crisp.Parsing
{
    class Scanner
    {
        string code;
        int i;
        int mark;
        char? current;
        char? peek;
        Position position;
        Position markPosition;

        Dictionary<string, TokenTag> keywords =
            new Dictionary<string, TokenTag>
            {
                ["else"]     = TokenTag.Else,
                ["false"]    = TokenTag.False,
                ["fn"]       = TokenTag.Fn,
                ["if"]       = TokenTag.If,
                ["let"]      = TokenTag.Let,
                ["null"]     = TokenTag.Null,
                ["rec"]      = TokenTag.Rec,
                ["true"]     = TokenTag.True,
                ["while"]    = TokenTag.While,
                ["write"]    = TokenTag.Write,
            };

        public Scanner(string code)
        {
            this.code = code;
            i = 0;
            mark = 0;
            current = GetChar(code, i);
            peek = GetChar(code, i + 1);
            position = current.HasValue
                ? new Position(Line: 1, Column: 1)
                : new Position(Line: 1, Column: 0);
            markPosition = position;
        }

        static char? GetChar(string s, int i)
        {
            return i < s.Length ? s[i] : (char?)null;
        }

        void Next(int delta = 1)
        {
            for (var j = 0; j < delta; j++)
            {
                position = current == '\n'
                    ? position.IncreaseLine()
                    : position.IncreaseColumn();
                i++;
                current = GetChar(code, i);
                peek = GetChar(code, i + 1);
            }
        }

        void Mark()
        {
            mark = i;
            markPosition = position;
        }

        Token Accept(TokenTag tag, int delta = 1)
        {
            var token = new Token(code.Substring(i, delta), tag, position);
            Next(delta);
            return token;
        }

        Token AcceptMark(TokenTag tag = TokenTag.Unknown)
        {
            return new Token(code.Substring(mark, i - mark), tag,
                markPosition);
        }

        public Token NextToken()
        {
            // Eat up comments and whitespace
            while (true)
            {
                while (current.HasValue && char.IsWhiteSpace(current.Value))
                {
                    Next();
                }
                if (current == '/' && peek == '/')
                {
                    Next(2);
                    while (current.HasValue && current != '\n')
                    {
                        Next();
                    }
                }
                else
                {
                    break;
                }
            }

            switch (current)
            {
                case null: return Accept(TokenTag.EndOfInput, 0);
                case '=' when peek == '=': return Accept(TokenTag.Equals, 2);
                case '!' when peek == '=': return Accept(TokenTag.InequalTo, 2);
                case '<' when peek == '=': return Accept(TokenTag.LessThanOrEqualTo, 2);
                case '>' when peek == '=': return Accept(TokenTag.GreaterThanOrEqualTo, 2);
                case '&' when peek == '&': return Accept(TokenTag.And, 2);
                case '|' when peek == '|': return Accept(TokenTag.Or, 2);
                case '{': return Accept(TokenTag.LBrace);
                case '}': return Accept(TokenTag.RBrace);
                case '(': return Accept(TokenTag.LParen);
                case ')': return Accept(TokenTag.RParen);
                case ',': return Accept(TokenTag.Comma);
                case '=': return Accept(TokenTag.Assignment);
                case '+': return Accept(TokenTag.Add);
                case '-': return Accept(TokenTag.Subtract);
                case '*': return Accept(TokenTag.Multiply);
                case '/': return Accept(TokenTag.Divide);
                case '<': return Accept(TokenTag.LessThan);
                case '>': return Accept(TokenTag.GreaterThan);
                case '!': return Accept(TokenTag.Bang);
                case '%': return Accept(TokenTag.Mod);

                case '\"':
                    {
                        Mark();
                        Next();
                        while (current.HasValue && current != '\"')
                        {
                            Next();
                        }
                        if (current == null)
                        {
                            throw new SyntaxErrorException("unexpected end of input", position);
                        }
                        Next();
                        return AcceptMark(TokenTag.String);
                    }

                case char c when char.IsDigit(c):
                    {
                        Mark();
                        while (current.HasValue && char.IsDigit(current.Value))
                        {
                            Next();
                        }
                        if (current == '.' && peek.HasValue && char.IsDigit(peek.Value))
                        {
                            do
                            {
                                Next();
                            }
                            while (current.HasValue && char.IsDigit(current.Value));
                        }
                        return AcceptMark(TokenTag.Number);
                    }

                case char c when char.IsLetter(c):
                    {
                        Mark();
                        while (current.HasValue && char.IsLetterOrDigit(current.Value))
                        {
                            Next();
                        }
                        var token = AcceptMark();
                        if (!keywords.TryGetValue(token.Lexeme, out var tag))
                        {
                            tag = TokenTag.Identifier;
                        }
                        return new Token(token.Lexeme, tag, token.Position);
                    }

                default:
                    throw new SyntaxErrorException(
                        $"unexpected character '{current}'",
                        position);
            }
        }
    }
}
