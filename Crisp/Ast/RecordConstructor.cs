using Crisp.Eval;
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

        public object Evaluate(Environment environment)
        {
            var rec = Record.Evaluate(environment) as Eval.Record;
            if (rec == null)
            {
                throw new RuntimeErrorException(
                    Position,
                    "Record construction requires a record object.");
            }
            var members = Initializers.MapDictionary((name, expr) => expr.Evaluate(environment));
            return rec.Construct(members);
        }
    }
}
