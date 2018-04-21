using System.Collections.Generic;

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
    }
}
