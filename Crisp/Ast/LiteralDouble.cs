using Crisp.Runtime;

namespace Crisp.Ast
{
    class LiteralDouble : IExpression
    {
        public double Value { get; }

        public LiteralDouble(double value)
        {
            Value = value;
        }

        public object Evaluate(Environment environment)
        {
            return Value;
        }
    }
}
