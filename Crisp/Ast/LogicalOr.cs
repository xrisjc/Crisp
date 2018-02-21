namespace Crisp.Ast
{
    class LogicalOr : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public LogicalOr(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }
    }
}
