using System.Collections.Generic;
using System.Linq;
using Crisp.Eval;

namespace Crisp.Ast
{
    class Block : IExpression
    {
        public IEnumerable<IExpression> Body { get; }

        public Block(IEnumerable<IExpression> body)
        {
            Body = body;
        }

        public object Evaluate(Environment environment)
        {
            var localEnvironment = new Environment(environment);
            return Body.Evaluate(localEnvironment).LastOrDefault() ?? Null.Instance;
        }
    }
}
