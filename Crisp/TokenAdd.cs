namespace Crisp
{
    class TokenAdd : TokenInfixOperator
    {
        public override Precidence Lbp => Precidence.Additive;

        public override IExpression CreateExpression(IExpression left, IExpression right)
        {
            return new ExpressionAdd(left, right);
        }
    }
}
