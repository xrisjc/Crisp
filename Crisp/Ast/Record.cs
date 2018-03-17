using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public IEnumerable<Identifier> Members { get; }

        public Record(IEnumerable<Identifier> members)
        {
            Members = members;
        }

        public Record()
            : this(Enumerable.Empty<Identifier>())
        {
        }
    }
}
