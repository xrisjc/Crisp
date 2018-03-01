using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function : IExpression
    {
        public List<Identifier> Parameters { get; }

        public IExpression Body { get; }

        public Function(List<Identifier> parameters, IExpression body)
        {
            Body = body;
            Parameters = parameters;
        }
    }
}
