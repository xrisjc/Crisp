using System.Collections.Generic;

namespace Crisp.Ast
{
    class RecordConstructor : IExpression
    {
        public IExpression Record { get; }

        public Dictionary<string, IExpression> Initializers { get; }

        public RecordConstructor(IExpression record, Dictionary<string, IExpression> initializers)
        {
            Record = record;
            Initializers = initializers;
        }

        public RecordConstructor(IExpression record)
            : this(record, new Dictionary<string, IExpression>())
        {
        }
    }
}
