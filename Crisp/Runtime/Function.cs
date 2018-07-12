using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Function
    {
        public IExpression Body { get; }

        public List<string> Parameters { get; }

        public Environment Environment { get; }

        public Function(List<string> parameters, IExpression body, Environment environment)
        {
            Parameters = parameters;
            Body = body;
            Environment = environment;
        }
    }
}
