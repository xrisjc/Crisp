﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp
{
    class Lexer
    {
        readonly string code;
        int i = -1;
        char current = '\0';
        char peek = '\0';

        public Lexer(string code)
        {
            this.code = code;
            Next();
        }

        public Token NextToken()
        {
            while (i < code.Length && char.IsWhiteSpace(current))
            {
                Next();
            }

            switch (current)
            {
                case ':' when peek == '=':
                    Next(2);
                    return new TokenAssignment();

                case '\0':
                    Next();
                    return new TokenEndOfInput();

                case '(':
                    Next();
                    return new TokenLParen();

                case ')':
                    Next();
                    return new TokenRParen();

                case '{':
                    Next();
                    return new TokenLBrace();

                case '}':
                    Next();
                    return new TokenRBrace();

                case ',':
                    Next();
                    return new TokenComma();

                case ';':
                    Next();
                    return new TokenSemicolon();

                case '=':
                    Next();
                    return new TokenEquals();

                case '+':
                    Next();
                    return new TokenAdd();

                case '*':
                    Next();
                    return new TokenMultiply();

                case char c when char.IsDigit(c):
                    {
                        var sb = new StringBuilder();
                        while (i < code.Length && char.IsDigit(current))
                        {
                            sb.Append(current);
                            Next();
                        }
                        var tokenText = sb.ToString();
                        if (long.TryParse(tokenText, out long value))
                        {
                            return new TokenLiteralInteger(value); 
                        }
                        else
                        {
                            throw new SyntaxErrorException(
                                $"unable to convert '{tokenText}' to an integer");
                        }
                    }

                case char c when char.IsLetter(c):
                    {
                        var sb = new StringBuilder();
                        while (i < code.Length && char.IsLetter(current))
                        {
                            sb.Append(current);
                            Next();
                        }
                        var tokenText = sb.ToString();

                        switch (tokenText)
                        {
                            case "fn":
                                return new TokenFn();
                            case "let":
                                return new TokenLet();
                            default:
                                return new TokenIdentifier(tokenText);
                        }

                    }

                default:
                    throw new SyntaxErrorException($"unexpected character {current}");
            }
        }

        void Next(int delta = 1)
        {
            char GetChar(int j) => (j < code.Length) ? code[j] : '\0';
            i += delta;
            current = GetChar(i);
            peek = GetChar(i + 1);
        }
    }
}
