using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function : IExpression
    {
        public IEnumerable<string> Parameters { get; }

        public IExpression Body { get; }

        public Function(IEnumerable<string> parameters, IExpression body)
        {
            Body = body;
            Parameters = parameters;
        }
    }
}
