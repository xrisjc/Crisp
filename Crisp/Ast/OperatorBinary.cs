using Crisp.Eval;
using Crisp.Parsing;
using static Crisp.Eval.Utility;

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

        public object Evaluate(Environment environment)
        {
            if (Op == OperatorInfix.And)
            {
                return IsTrue(Left.Evaluate(environment)) && IsTrue(Right.Evaluate(environment));
            }

            if (Op == OperatorInfix.Or)
            {
                return IsTrue(Left.Evaluate(environment)) || IsTrue(Right.Evaluate(environment));
            }

            dynamic left = Left.Evaluate(environment);
            dynamic right = Right.Evaluate(environment);

            switch (Op)
            {
                case OperatorInfix.Add when left is string && right is string:
                    return string.Concat(left, right);

                case OperatorInfix.Add when CheckNumeric(left, right):
                    return left + right;

                case OperatorInfix.Sub when CheckNumeric(left, right):
                    return left - right;

                case OperatorInfix.Mul when CheckNumeric(left, right):
                    return left * right;

                case OperatorInfix.Div when CheckNumeric(left, right):
                    return left / right;

                case OperatorInfix.Mod when CheckNumeric(left, right):
                    return left % right;

                case OperatorInfix.Lt when CheckNumeric(left, right):
                    return left < right;

                case OperatorInfix.LtEq when CheckNumeric(left, right):
                    return left <= right;

                case OperatorInfix.Gt when CheckNumeric(left, right):
                    return left > right;

                case OperatorInfix.GtEq when CheckNumeric(left, right):
                    return left >= right;

                case OperatorInfix.Eq when CheckNumeric(left, right):
                    // If left is int and right is double then left.Equals(right) may not
                    // be the same as right.Equals(left).  I suppose it's something to do with
                    // calinging Int32's Equals.  == seems to work for numbers, though.
                    return left == right;

                case OperatorInfix.Eq:
                    return left.Equals(right);

                case OperatorInfix.Neq:
                    return !left.Equals(right);

                default:
                    throw new RuntimeErrorException(
                        Position,
                        $"Operator {Op} cannot be applied to values " +
                        $"<{left}> and <{right}>");
            }
        }
    }
}
