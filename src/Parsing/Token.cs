namespace Crisp.Parsing
{
    class Token
    {
        public string Lexeme { get; }

        public TokenTag Tag { get; }

        public Position Position { get; }

        public Token(string lexeme, TokenTag tag, Position position)
        {
            Lexeme = lexeme;
            Tag = tag;
            Position = position;
        }

        public Token()
            : this("", TokenTag.Unknown, new Position(0, 0))
        {
        }

        public static implicit operator bool(Token? token) => token != null;

        public override string ToString()
        {
            return $"({Tag}, <{Lexeme}>, {Position})";
        }
    }
}
