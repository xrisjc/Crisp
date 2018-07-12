using System.Collections.Generic;
using System.Linq;
using Crisp.Runtime;

namespace Crisp.Ast
{
    class List : IExpression
    {
        public IEnumerable<IExpression> Initializers { get; }

        public List(IEnumerable<IExpression> initializers)
        {
            Initializers = initializers;
        }

        public List()
            : this(Enumerable.Empty<IExpression>())
        {
        }

        public object Evaluate(Environment environment)
        {
            return Initializers.Evaluate(environment).ToList();
        }
    }
}
