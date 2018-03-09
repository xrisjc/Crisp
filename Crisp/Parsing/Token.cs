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

        public Token(TokenTag tag, Position position)
            : this("", tag, position)
        {
        }

        public override string ToString()
        {
            return $"Lexeme = {Lexeme}, Tag = {Tag}, Position = ({Position})";
        }
    }
}
