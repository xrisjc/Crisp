using Crisp.Parsing;

namespace Crisp.Ast
{
    class OperatorBinary : IExpression
    {
        public Position Position { get; }

        public OperatorBinaryTag Tag { get; }

        public IExpression Left { get; }

        public IExpression Right { get; }

        public OperatorBinary(Position position, OperatorBinaryTag tag, IExpression left, IExpression right)
        {
            Position = position;
            Tag = tag;
            Left = left;
            Right = right;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
