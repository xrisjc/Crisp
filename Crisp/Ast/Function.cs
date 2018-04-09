using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function : IExpression
    {
        public IEnumerable<Identifier> Parameters { get; }

        public IExpression Body { get; }

        public Function(IEnumerable<Identifier> parameters, IExpression body)
        {
            Body = body;
            Parameters = parameters;
        }
    }
}
