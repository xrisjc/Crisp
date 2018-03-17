using System.Collections.Generic;
using System.Linq;

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
    }
}
