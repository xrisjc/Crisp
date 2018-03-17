using System.Collections.Generic;
using System.Linq;

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
    }
}
