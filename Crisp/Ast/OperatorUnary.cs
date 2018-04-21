using Crisp.Parsing;

namespace Crisp.Ast
{
    class OperatorUnary : IExpression
    {
        public Position Position { get; }

        public OperatorPrefix Op { get; }

        public IExpression Expression { get; }

        public OperatorUnary(Position position, OperatorPrefix op, IExpression expression)
        {
            Position = position;
            Op = op;
            Expression = expression;
        }
    }
}
