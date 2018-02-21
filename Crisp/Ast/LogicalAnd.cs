namespace Crisp.Ast
{
    class LogicalAnd : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public LogicalAnd(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }
    }
}
