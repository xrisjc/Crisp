using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class RecordConstructor : IExpression
    {
        public IExpression Record { get; }

        public IEnumerable<(Identifier, IExpression)> Initializers { get; }

        public RecordConstructor(IExpression record, IEnumerable<(Identifier, IExpression)> initializers)
        {
            Record = record;
            Initializers = initializers;
        }

        public RecordConstructor(IExpression record)
            : this(record, Enumerable.Empty<(Identifier, IExpression)>())
        {
        }
    }
}
