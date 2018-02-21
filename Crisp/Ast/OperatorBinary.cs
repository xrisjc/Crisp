namespace Crisp.Ast
{
    class OperatorBinary : IExpression
    {
        public Operator Op { get; }

        public IExpression Left { get; }

        public IExpression Right { get; }

        public OperatorBinary(Operator op, IExpression left, IExpression right)
        {
            Op = op;
            Left = left;
            Right = right;
        }
    }
}
