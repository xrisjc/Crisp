namespace Crisp
{
    class TokenEquals : TokenInfixOperator
    {
        public override Precidence Lbp => Precidence.Equality;

        public override IExpression CreateExpression(IExpression left, IExpression right)
        {
            return new ExpressionEquals(left, right);
        }
    }
}
