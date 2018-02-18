using System.Text;

namespace Crisp.Parsing
{
    class Lexer
    {
        readonly string code;
        int i;
        char? current;
        char? peek;
        Position position;

        public Lexer(string code)
        {
            this.code = code;
            i = 0;
            current = GetChar(code, i);
            peek = GetChar(code, i + 1);
            position = current.HasValue
                ? new Position(line: 1, column: 1)
                : new Position(line: 1, column: 0);
        }

        static char? GetChar(string s, int i)
        {
            return i < s.Length ? s[i] : (char?)null;
        }

        void Next(int times = 1)
        {
            for (int j = 0; j < times; j++)
            {
                position = current == '\n'
                    ? position.IncreaseLine()
                    : position.IncreaseColumn();
                i++;
                current = GetChar(code, i);
                peek = GetChar(code, i + 1);
            }
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
                case null:
                    return new Token(TokenTag.EndOfInput, position);

                case ':' when peek == '=':
                    Next(2);
                    return new Token(TokenTag.Assignment, position);

                case '<' when peek == '>':
                    Next(2);
                    return new Token(TokenTag.InequalTo, position);

                case '<' when peek == '=':
                    Next(2);
                    return new Token(TokenTag.LessThanOrEqualTo, position);

                case '>' when peek == '=':
                    Next(2);
                    return new Token(TokenTag.GreaterThanOrEqualTo, position);

                case '(':
                    Next();
                    return new Token(TokenTag.LParen, position);

                case ')':
                    Next();
                    return new Token(TokenTag.RParen, position);

                case '[':
                    Next();
                    return new Token(TokenTag.LBracket, position);

                case ']':
                    Next();
                    return new Token(TokenTag.RBracket, position);

                case '{':
                    Next();
                    return new Token(TokenTag.LBrace, position);

                case '}':
                    Next();
                    return new Token(TokenTag.RBrace, position);

                case ',':
                    Next();
                    return new Token(TokenTag.Comma, position);

                case '=':
                    Next();
                    return new Token(TokenTag.Equals, position);

                case '+':
                    Next();
                    return new Token(TokenTag.Add, position);

                case '-':
                    Next();
                    return new Token(TokenTag.Subtract, position);

                case '*':
                    Next();
                    return new Token(TokenTag.Multiply, position);

                case '/':
                    Next();
                    return new Token(TokenTag.Divide, position);

                case '%':
                    Next();
                    return new Token(TokenTag.Modulo, position);

                case '<':
                    Next();
                    return new Token(TokenTag.LessThan, position);

                case '>':
                    Next();
                    return new Token(TokenTag.GreaterThan, position);

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
                        return new TokenValue<string>(
                            TokenTag.String,
                            position,
                            value: sb.ToString());
                    }

                case char c when char.IsDigit(c):
                    {
                        var sb = new StringBuilder();
                        while (current.HasValue &&
                               char.IsDigit(current.Value))
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
                            while (current.HasValue && 
                                   char.IsDigit(current.Value));
                        }
                        var tokenText = sb.ToString();
                        if (double.TryParse(tokenText, out var value))
                        {
                            return new TokenValue<double>(
                                TokenTag.Number,
                                position,
                                value: value);
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
                        while (current.HasValue &&
                               char.IsLetter(current.Value))
                        {
                            sb.Append(current);
                            Next();
                        }
                        var tokenText = sb.ToString();

                        switch (tokenText)
                        {
                            case "and":
                                return new Token(TokenTag.And, position);
                            case "begin":
                                return new Token(TokenTag.Begin, position);
                            case "do":
                                return new Token(TokenTag.Do, position);
                            case "else":
                                return new Token(TokenTag.Else, position);
                            case "end":
                                return new Token(TokenTag.End, position);
                            case "false":
                                return new Token(TokenTag.False, position);
                            case "fn":
                                return new Token(TokenTag.Fn, position);
                            case "if":
                                return new Token(TokenTag.If, position);
                            case "let":
                                return new Token(TokenTag.Let, position);
                            case "null":
                                return new Token(TokenTag.Null, position);
                            case "or":
                                return new Token(TokenTag.Or, position);
                            case "then":
                                return new Token(TokenTag.Then, position);
                            case "true":
                                return new Token(TokenTag.True, position);
                            case "while":
                                return new Token(TokenTag.While, position);
                            default:
                                return new TokenValue<string>(
                                    TokenTag.Identifier,
                                    position,
                                    value: tokenText);
                        }

                    }

                default:
                    throw new SyntaxErrorException(
                        $"unexpected character '{current}'");
            }
        }
    }
}
