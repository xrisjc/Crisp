namespace Crisp
{
    class TokenMultiply : TokenInfixOperator
    {
        public override Precidence Lbp => Precidence.Multiplicitive;

        public override IExpression CreateExpression(IExpression left, IExpression right)
        {
            return new ExpressionMultiply(left, right);
        }
    }
}
