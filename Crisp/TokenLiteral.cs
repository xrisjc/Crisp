namespace Crisp
{
    class TokenLiteral<T> : Token
    {
        T value;

        public TokenLiteral(T value)
        {
            this.value = value;
        }

        public override IExpression Nud(Parser parser)
        {
            return new ExpressionLiteral<T>(value);
        }
    }

    static class TokenLiteral
    {
        public static TokenLiteral<T> Create<T>(T value)
        {
            return new TokenLiteral<T>(value);
        }
    }
}
