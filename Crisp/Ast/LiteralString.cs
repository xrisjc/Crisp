using Crisp.Runtime;

namespace Crisp.Ast
{
    class LiteralString : IExpression
    {
        public string Value { get; }

        public LiteralString(string value)
        {
            Value = value;
        }

        public object Evaluate(Environment environment)
        {
            return Value;
        }
    }
}
