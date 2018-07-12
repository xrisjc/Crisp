using Crisp.Runtime;
using Crisp.Parsing;
using static Crisp.Runtime.Utility;

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

        public object Evaluate(Environment environment)
        {
            dynamic obj = Expression.Evaluate(environment);

            switch (Op)
            {
                case OperatorPrefix.Neg when obj is int || obj is double:
                    return -obj;
                case OperatorPrefix.Not:
                    return !IsTrue(obj);
                default:
                    throw new RuntimeErrorException(
                        Position,
                        $"Operator {Op} cannot be applied to value <{obj}>");
            }
        }
    }
}
