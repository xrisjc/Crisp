using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class RecordConstructor : IExpression
    {
        public Position Position { get; }

        public IExpression Record { get; }

        public Dictionary<string, IExpression> Initializers { get; }

        public RecordConstructor(Position position, IExpression record, Dictionary<string, IExpression> initializers)
        {
            Position = position;
            Record = record;
            Initializers = initializers;
        }
    }
}
