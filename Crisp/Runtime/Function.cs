using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Function
    {
        public List<IExpression> Body { get; }

        public List<string> Parameters { get; }

        public Environment Environment { get; }

        public Function(List<string> parameters, List<IExpression> body, Environment environment)
        {
            Parameters = parameters;
            Body = body;
            Environment = environment;
        }
    }
}
