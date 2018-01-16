namespace Crisp
{
    class TokenSemicolon : TokenInfixOperator
    {
        public override Precidence Lbp => Precidence.Sequence;

        public override IExpression CreateExpression(IExpression left, IExpression right)
        {
            return new ExpressionSequence(left, right);
        }
    }
}
