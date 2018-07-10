using System.Collections.Generic;
using Crisp.Eval;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public List<string> Variables { get; }

        public Dictionary<string, Function> Functions { get; }

        public Record(List<string> variables, Dictionary<string, Function> functions)
        {
            Variables = variables;
            Functions = functions;
        }

        public Record()
            : this(new List<string>(), new Dictionary<string, Function>())
        {
        }

        public object Evaluate(Environment environment)
        {
            return new Eval.Record(
                Variables,
                Functions.MapDictionary(
                    (name, fn) => new Eval.Function(fn.Parameters, fn.Body, environment)));
        }
    }
}
