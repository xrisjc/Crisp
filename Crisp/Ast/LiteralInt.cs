using Crisp.Runtime;

namespace Crisp.Ast
{
    class LiteralInt : IExpression
    {
        public int Value { get; }

        public LiteralInt(int value)
        {
            Value = value;
        }

        public object Evaluate(Environment environment)
        {
            return Value;
        }
    }
}
