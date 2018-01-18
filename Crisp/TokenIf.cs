namespace Crisp
{
    class TokenIf : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var condition = parser.ParseExpression();
            var consequence = parser.ParseExpression();
            var alternative = parser.Match<TokenElse>()
                ? parser.ParseExpression()
                : ExpressionLiteralNull.Instance;

            return new ExpressionBranch(condition, consequence, alternative);
        }
    }
}
