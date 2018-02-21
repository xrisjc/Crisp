using Crisp.Eval;

namespace Crisp.Ast
{
    class OperatorBinary : IExpression
    {
        public IOperatorBinary Op { get; }

        public IExpression Left { get; }

        public IExpression Right { get; }

        public OperatorBinary(IOperatorBinary op, IExpression left, IExpression right)
        {
            Op = op;
            Left = left;
            Right = right;
        }
    }
}
