using System.Collections.Generic;
using Crisp.Runtime;

namespace Crisp.Ast
{
    class Function : IExpression
    {
        public List<string> Parameters { get; }

        public IExpression Body { get; }

        public Function(List<string> parameters, IExpression body)
        {
            Parameters = parameters;
            Body = body;
        }

        public object Evaluate(Environment environment)
        {
            return new Runtime.Function(Parameters, Body, environment);
        }
    }
}
