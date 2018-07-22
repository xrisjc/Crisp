using Crisp.Parsing;

namespace Crisp.Ast
{
    class OperatorBinary : IExpression
    {
        public Position Position { get; }

        public OperatorInfix Op { get; }

        public IExpression Left { get; }

        public IExpression Right { get; }

        public OperatorBinary(Position position, OperatorInfix op, IExpression left, IExpression right)
        {
            Position = position;
            Op = op;
            Left = left;
            Right = right;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
