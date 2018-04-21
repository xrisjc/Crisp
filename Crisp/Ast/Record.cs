using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public IEnumerable<string> Variables { get; }

        public IEnumerable<NamedFunction> Functions { get; }

        public Record(IEnumerable<string> variables, IEnumerable<NamedFunction> functions)
        {
            Variables = variables;
            Functions = functions;
        }

        public Record()
            : this(Enumerable.Empty<string>(), Enumerable.Empty<NamedFunction>())
        {
        }
    }
}
