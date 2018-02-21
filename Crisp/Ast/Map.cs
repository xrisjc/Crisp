using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Map : IExpression
    {
        public IEnumerable<IndexValuePair> Initializers { get; }

        public Map(IEnumerable<IndexValuePair> initializers)
        {
            Initializers = initializers;
        }

        public Map()
            : this(Enumerable.Empty<IndexValuePair>())
        {
        }
    }
}
