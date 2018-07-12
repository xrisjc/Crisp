using Crisp.Runtime;

namespace Crisp.Ast
{
    class LiteralNull : IExpression
    {
        private LiteralNull() { }

        public static LiteralNull Instance { get; } = new LiteralNull();

        public object Evaluate(Environment environment)
        {
            return Runtime.Null.Instance;
        }
    }
}
