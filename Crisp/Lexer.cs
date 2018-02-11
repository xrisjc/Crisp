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
            // Eat up comments and whitespace
            while (true)
            {
                while (i < code.Length && char.IsWhiteSpace(current))
                {
                    Next();
                }
                if (current == '/' && peek == '/')
                {
                    Next(2);
                    while (i < code.Length && current != '\n')
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
                case ':' when peek == '=':
                    Next(2);
                    return new TokenAssignment();

                case '<' when peek == '>':
                    Next(2);
                    return new TokenInequalTo();

                case '<' when peek == '=':
                    Next(2);
                    return new TokenLessThanOrEqualTo();

                case '>' when peek == '=':
                    Next(2);
                    return new TokenGreaterThanOrEqualTo();

                case '\0':
                    Next();
                    return new TokenEndOfInput();

                case '(':
                    Next();
                    return new TokenLParen();

                case ')':
                    Next();
                    return new TokenRParen();

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

                case '/':
                    Next();
                    return new TokenDivide();

                case '%':
                    Next();
                    return new TokenModulo();

                case '<':
                    Next();
                    return new TokenLessThan();

                case '>':
                    Next();
                    return new TokenGreaterThan();

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
                        if (current == '.')
                        {
                            do
                            {
                                sb.Append(current);
                                Next();
                            }
                            while (i < code.Length && char.IsDigit(current));
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
                            case "and":
                                return new TokenAnd();
                            case "begin":
                                return new TokenBegin();
                            case "do":
                                return new TokenDo();
                            case "else":
                                return new TokenElse();
                            case "end":
                                return new TokenEnd();
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
                            case "or":
                                return new TokenOr();
                            case "then":
                                return new TokenThen();
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
