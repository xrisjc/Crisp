using Crisp.Eval;

namespace Crisp.Ast
{
    class While : IExpression
    {
        public IExpression Guard { get; }

        public IExpression Body { get; }

        public While(IExpression guard, IExpression body)
        {
            Guard = guard;
            Body = body;
        }

        public object Evaluate(Environment environment)
        {
            while (Eval.Utility.IsTrue(Guard.Evaluate(environment)))
            {
                Body.Evaluate(environment);
            }
            return Null.Instance;
        }
    }
}
