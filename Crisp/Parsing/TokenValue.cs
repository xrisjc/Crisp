namespace Crisp.Parsing
{
    class TokenValue<T> : Token
    {
        public T Value { get; }

        public TokenValue(TokenTag tag, Position position, T value)
            : base(tag, position)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Value = {Value}";
        }
    }
}
