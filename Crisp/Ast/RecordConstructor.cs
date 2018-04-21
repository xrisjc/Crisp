using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class RecordConstructor : IExpression
    {
        public IExpression Record { get; }

        public IEnumerable<(string, IExpression)> Initializers { get; }

        public RecordConstructor(IExpression record, IEnumerable<(string, IExpression)> initializers)
        {
            Record = record;
            Initializers = initializers;
        }

        public RecordConstructor(IExpression record)
            : this(record, Enumerable.Empty<(string, IExpression)>())
        {
        }
    }
}
