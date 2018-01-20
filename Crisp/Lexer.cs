using System.Text;

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

                case '=':
                    Next();
                    return new TokenEquals();

                case '+':
                    Next();
                    return new TokenAdd();

                case '-':
                    Next();
                    return new TokenSubtract();

                case '*':
                    Next();
                    return new TokenMultiply();

                case '\'':
                    {
                        var sb = new StringBuilder();
                        Next();
                        while (i < code.Length && current != '\'')
                        {
                            sb.Append(current);
                            Next();
                        }
                        if (i == code.Length)
                        {
                            throw new SyntaxErrorException("unexpected end of input");
                        }
                        Next();
                        return TokenLiteral.Create(sb.ToString());
                    }

                case char c when char.IsDigit(c):
                    {
                        var sb = new StringBuilder();
                        while (i < code.Length && char.IsDigit(current))
                        {
                            sb.Append(current);
                            Next();
                        }
                        var tokenText = sb.ToString();
                        if (double.TryParse(tokenText, out var value))
                        {
                            return TokenLiteral.Create(value); 
                        }
                        else
                        {
                            throw new SyntaxErrorException(
                                $"unable to convert '{tokenText}' to a number");
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
                            case "else":
                                return new TokenElse();
                            case "false":
                                return TokenLiteral.Create(false);
                            case "fn":
                                return new TokenFn();
                            case "if":
                                return new TokenIf();
                            case "let":
                                return new TokenLet();
                            case "null":
                                return new TokenNull();
                            case "true":
                                return TokenLiteral.Create(true);
                            case "while":
                                return new TokenWhile();
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
