using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class ForIn : IExpression
    {
        public string VariableName { get; }

        public IExpression Sequence { get; }

        public IExpression Body { get; }

        public ForIn(string variableName, IExpression sequence, IExpression body)
        {
            VariableName = variableName;
            Sequence = sequence;
            Body = body;
        }

        public object Evaluate(Environment environment)
        {
            switch (Sequence.Evaluate(environment))
            {
                case List<object> list:
                    foreach (var x in list)
                    {
                        var localEnvironment = new Environment(environment);
                        localEnvironment.Create(VariableName, x);
                        Body.Evaluate(localEnvironment);
                    }
                    break;

                default:
                    throw new RuntimeErrorException(
                        $"For in loops must have an enumerable object.");
            }
            return Null.Instance;
        }
    }
}
