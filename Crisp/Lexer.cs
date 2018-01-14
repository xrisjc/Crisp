using System;
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
                case '\0':
                    Next();
                    return new TokenEndOfInput();

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

                default:
                    throw new SyntaxErrorException($"unexpected character {current}");
            }
        }

        void Next()
        {
            char GetChar(int j) => (j < code.Length) ? code[j] : '\0';
            i++;
            current = GetChar(i);
            peek = GetChar(i + 1);
        }
    }
}
