using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public IEnumerable<string> Variables { get; }

        public Dictionary<string, Function> Functions { get; }

        public Record(IEnumerable<string> variables, Dictionary<string, Function> functions)
        {
            Variables = variables;
            Functions = functions;
        }

        public Record()
            : this(Enumerable.Empty<string>(), new Dictionary<string, Function>())
        {
        }
    }
}
