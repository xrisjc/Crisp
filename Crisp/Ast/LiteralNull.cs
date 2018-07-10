using Crisp.Eval;

namespace Crisp.Ast
{
    class LiteralNull : IExpression
    {
        private LiteralNull() { }

        public static LiteralNull Instance { get; } = new LiteralNull();

        public object Evaluate(Environment environment)
        {
            return Eval.Null.Instance;
        }
    }
}
