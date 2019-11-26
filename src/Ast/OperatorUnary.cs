using Crisp.Parsing;

namespace Crisp.Ast
{
    class OperatorUnary : IExpression
    {
        public Position Position { get; }

        public OperatorUnaryTag Op { get; }

        public IExpression Expression { get; }

        public OperatorUnary(Position position, OperatorUnaryTag op, IExpression expression)
        {
            Position = position;
            Op = op;
            Expression = expression;
        }
    }
}
