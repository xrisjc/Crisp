using Crisp.Eval;

namespace Crisp.Ast
{
    class LiteralBool : IExpression
    {
        public bool Value { get; }

        public LiteralBool(bool value)
        {
            Value = value;
        }

        public static LiteralBool True { get; } = new LiteralBool(true);

        public static LiteralBool False { get; } = new LiteralBool(false);

        public object Evaluate(Environment environment)
        {
            return Value;
        }
    }
}
