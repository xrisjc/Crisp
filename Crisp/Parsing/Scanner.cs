using System.Text;

namespace Crisp.Parsing
{
    class Scanner
    {
        readonly string code;
        int i;
        char? current;
        char? peek;
        Position position;

        public Scanner(string code)
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

        Token Accept(TokenTag tag, int delta = 1)
        {
            var token = new Token(tag, position);
            Next(delta);
            return token;
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
                        var startPosition = position;
                        var sb = new StringBuilder();
                        Next();
                        while (current.HasValue && current != '\'')
                        {
                            sb.Append(current);
                            Next();
                        }
                        if (current == null)
                        {
                            throw new SyntaxErrorException("unexpected end of input");
                        }
                        Next();
                        return new TokenValue<string>(TokenTag.String, startPosition, sb.ToString());
                    }

                case char c when char.IsDigit(c):
                    {
                        var startPosition = position;
                        var sb = new StringBuilder();
                        while (current.HasValue && char.IsDigit(current.Value))
                        {
                            sb.Append(current);
                            Next();
                        }
                        if (current == '.' && peek.HasValue && char.IsDigit(peek.Value))
                        {
                            do
                            {
                                sb.Append(current);
                                Next();
                            }
                            while (current.HasValue && char.IsDigit(current.Value));
                        }
                        var tokenText = sb.ToString();
                        if (int.TryParse(tokenText, out var intValue))
                        {
                            return new TokenValue<int>(TokenTag.Integer, startPosition, intValue);
                        }
                        else if (double.TryParse(tokenText, out var value))
                        {
                            return new TokenValue<double>(TokenTag.Float, startPosition, value);
                        }
                        else
                        {
                            throw new SyntaxErrorException(
                                $"unable to convert '{tokenText}' to int64 or float64");
                        }
                    }

                case char c when char.IsLetter(c):
                    {
                        var startPosition = position;
                        var sb = new StringBuilder();
                        while (current.HasValue && char.IsLetter(current.Value))
                        {
                            sb.Append(current);
                            Next();
                        }
                        var tokenText = sb.ToString();
                        switch (tokenText)
                        {
                            case "and":   return new Token(TokenTag.And,   startPosition);
                            case "begin": return new Token(TokenTag.Begin, startPosition);
                            case "do":    return new Token(TokenTag.Do,    startPosition);
                            case "else":  return new Token(TokenTag.Else,  startPosition);
                            case "end":   return new Token(TokenTag.End,   startPosition);
                            case "false": return new Token(TokenTag.False, startPosition);
                            case "fn":    return new Token(TokenTag.Fn,    startPosition);
                            case "if":    return new Token(TokenTag.If,    startPosition);
                            case "let":   return new Token(TokenTag.Let,   startPosition);
                            case "not":   return new Token(TokenTag.Not,   startPosition);
                            case "null":  return new Token(TokenTag.Null,  startPosition);
                            case "mod":   return new Token(TokenTag.Mod,   startPosition);
                            case "or":    return new Token(TokenTag.Or,    startPosition);
                            case "then":  return new Token(TokenTag.Then,  startPosition);                         
                            case "true":  return new Token(TokenTag.True,  startPosition);
                            case "while": return new Token(TokenTag.While, startPosition);
                            default:
                                return new TokenValue<string>(
                                    TokenTag.Identifier,
                                    startPosition,
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
