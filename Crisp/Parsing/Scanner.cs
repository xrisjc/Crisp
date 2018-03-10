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
                ["and"]    = TokenTag.And,
                ["begin"]  = TokenTag.Begin,
                ["do"]     = TokenTag.Do,
                ["else"]   = TokenTag.Else,
                ["end"]    = TokenTag.End,
                ["false"]  = TokenTag.False,
                ["fn"]     = TokenTag.Fn,
                ["if"]     = TokenTag.If,
                ["let"]    = TokenTag.Let,
                ["not"]    = TokenTag.Not,
                ["null"]   = TokenTag.Null,
                ["mod"]    = TokenTag.Mod,
                ["or"]     = TokenTag.Or,
                ["record"] = TokenTag.Record,
                ["then"]   = TokenTag.Then,
                ["true"]   = TokenTag.True,
                ["while"]  = TokenTag.While,
            };

        public Scanner(string code)
        {
            this.code = code;
            i = 0;
            mark = 0;
            current = GetChar(code, i);
            peek = GetChar(code, i + 1);
            position = current.HasValue
                ? new Position(line: 1, column: 1)
                : new Position(line: 1, column: 0);
            markPosition = position;
        }

        static char? GetChar(string s, int i)
        {
            return i < s.Length ? s[i] : (char?)null;
        }

        void Next(int delta = 1)
        {
            for (int j = 0; j < delta; j++)
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
            var token = new Token
            {
                Lexeme = code.Substring(i, delta),
                Tag = tag,
                Position = position
            };
            Next(delta);
            return token;
        }

        Token AcceptMark(TokenTag tag = TokenTag.Unknown)
        {
            return new Token
            {
                Lexeme = code.Substring(mark, i - mark),
                Tag = tag,
                Position = markPosition
            };
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
                case ':' when peek == '=':  return Accept(TokenTag.Assignment, 2);
                case '<' when peek == '>':  return Accept(TokenTag.InequalTo, 2);
                case '<' when peek == '=':  return Accept(TokenTag.LessThanOrEqualTo, 2);
                case '>' when peek == '=':  return Accept(TokenTag.GreaterThanOrEqualTo, 2);
                case '(': return Accept(TokenTag.LParen);
                case ')': return Accept(TokenTag.RParen);
                case '[': return Accept(TokenTag.LBracket);
                case ']': return Accept(TokenTag.RBracket);
                case '{': return Accept(TokenTag.LBrace);
                case '}': return Accept(TokenTag.RBrace);
                case ',': return Accept(TokenTag.Comma);
                case ':': return Accept(TokenTag.Colon);
                case '.': return Accept(TokenTag.Period);
                case '=': return Accept(TokenTag.Equals);
                case '+': return Accept(TokenTag.Add);
                case '-': return Accept(TokenTag.Subtract);
                case '*': return Accept(TokenTag.Multiply);
                case '/': return Accept(TokenTag.Divide);
                case '<': return Accept(TokenTag.LessThan);
                case '>': return Accept(TokenTag.GreaterThan);

                case '\'':
                    {
                        Next();
                        Mark();
                        while (current.HasValue && current != '\'')
                        {
                            Next();
                        }
                        if (current == null)
                        {
                            throw new SyntaxErrorException("unexpected end of input");
                        }
                        var token = AcceptMark(TokenTag.String);
                        Next();
                        return token;
                    }

                case char c when char.IsDigit(c):
                    {
                        Mark();
                        while (current.HasValue && char.IsDigit(current.Value))
                        {
                            Next();
                        }
                        TokenTag tag;
                        if (current == '.' && peek.HasValue && char.IsDigit(peek.Value))
                        {
                            do
                            {
                                Next();
                            }
                            while (current.HasValue && char.IsDigit(current.Value));
                            tag = TokenTag.Float;
                        }
                        else
                        {
                            tag = TokenTag.Integer;
                        }
                        return AcceptMark(tag);
                    }

                case char c when char.IsLetter(c):
                    {
                        Mark();
                        while (current.HasValue && char.IsLetter(current.Value))
                        {
                            Next();
                        }
                        var token = AcceptMark();
                        token.Tag = keywords.GetValue(token.Lexeme, TokenTag.Identifier);
                        return token;
                    }

                default:
                    throw new SyntaxErrorException(
                        $"unexpected character '{current}'");
            }
        }
    }
}
