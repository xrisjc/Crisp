namespace Crisp.Parsing
{
    class Token
    {
        public TokenTag Tag { get; }

        public Position Position { get; }

        public Token(TokenTag tag, Position position)
        {
            Tag = tag;
            Position = position;
        }

        public override string ToString()
        {
            return $"Tag = {Tag}, Position = ({Position})";
        }
    }
}
