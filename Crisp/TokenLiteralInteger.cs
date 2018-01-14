namespace Crisp
{
    class TokenLiteralInteger : Token
    {
        public long Value { get; }

        public TokenLiteralInteger(long value)
        {
            Value = value;
        }

        public override IExpression Nud(Parser parser)
        {
            return new ExpressionLiteralInteger(Value);
        }

        public override string ToString()
        {
            return $"LiteralInteger({Value})";
        }
    }
}