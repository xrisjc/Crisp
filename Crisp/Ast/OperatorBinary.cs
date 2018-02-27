namespace Crisp.Ast
{
    class OperatorBinary : IExpression
    {
        public OperatorInfix Op { get; }

        public IExpression Left { get; }

        public IExpression Right { get; }

        public OperatorBinary(OperatorInfix op, IExpression left, IExpression right)
        {
            Op = op;
            Left = left;
            Right = right;
        }
    }
}
