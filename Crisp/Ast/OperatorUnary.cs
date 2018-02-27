namespace Crisp.Ast
{
    class OperatorUnary : IExpression
    {
        public OperatorPrefix Op { get; }

        public IExpression Expression { get; }

        public OperatorUnary(OperatorPrefix op, IExpression expression)
        {
            Op = op;
            Expression = expression;
        }
    }
}
