using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public IEnumerable<Identifier> Variables { get; }

        public IEnumerable<NamedFunction> Functions { get; }

        public Record(IEnumerable<Identifier> variables, IEnumerable<NamedFunction> functions)
        {
            Variables = variables;
            Functions = functions;
        }

        public Record()
            : this(Enumerable.Empty<Identifier>(), Enumerable.Empty<NamedFunction>())
        {
        }
    }
}
