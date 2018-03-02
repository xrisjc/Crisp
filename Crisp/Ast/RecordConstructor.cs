using System.Collections.Generic;

namespace Crisp.Ast
{
    class RecordConstructor : IExpression
    {
        public IExpression Record { get; }

        public List<(Identifier, IExpression)> Initializers { get; }

        public RecordConstructor(IExpression record, List<(Identifier, IExpression)> initializers)
        {
            Record = record;
            Initializers = initializers;
        }

        public RecordConstructor(IExpression record)
            : this(record, new List<(Identifier, IExpression)>())
        {
        }
    }
}
