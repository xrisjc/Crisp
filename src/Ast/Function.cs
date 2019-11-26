using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function
    {
        public Identifier Name { get; }

        public List<string> Parameters { get; }

        public List<IExpression> Body { get; }

        public Function(Identifier name, List<string> parameters, List<IExpression> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
}
