using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public IEnumerable<Identifier> Members { get; }

        public IEnumerable<NamedFunction> Functions { get; }

        public Record(IEnumerable<Identifier> members, IEnumerable<NamedFunction> functions)
        {
            Members = members;
            Functions = functions;
        }

        public Record()
            : this(Enumerable.Empty<Identifier>(), Enumerable.Empty<NamedFunction>())
        {
        }
    }
}
