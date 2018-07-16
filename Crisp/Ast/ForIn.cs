using Crisp.Runtime;
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
            void LoopBody(object value)
            {
                var localEnvironment = new Environment(environment);
                localEnvironment.Create(VariableName, value);
                Body.Evaluate(localEnvironment);
            }

            switch (Sequence.Evaluate(environment))
            {
                case Runtime.List list:
                    foreach (var item in list)
                    {
                        LoopBody(item);
                    }
                    break;

                case Runtime.Map map:
                    foreach (var key in map.Keys)
                    {
                        LoopBody(key);
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
