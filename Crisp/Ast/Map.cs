using System.Collections.Generic;
using System.Linq;
using Crisp.Runtime;

namespace Crisp.Ast
{
    class Map : IExpression
    {
        public IEnumerable<(IExpression, IExpression)> Initializers { get; }

        public Map(IEnumerable<(IExpression, IExpression)> initializers)
        {
            Initializers = initializers;
        }

        public Map()
            : this(Enumerable.Empty<(IExpression, IExpression)>())
        {
        }

        public object Evaluate(Environment environment)
        {
            return Initializers.Evaluate(environment).CreateDictionary();
        }
    }
}
