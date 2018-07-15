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

        public object Evaluate(Environment environment)
        {
            var items = from initializer in Initializers
                        select initializer.Evaluate(environment);
            return new Runtime.List(items);
        }
    }
}
