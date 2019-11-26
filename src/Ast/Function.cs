using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function
    {
        public Identifier Name { get; }

        public List<string> Parameters { get; }

        public Block Body { get; }

        public Function(Identifier name, List<string> parameters, Block body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
}
