using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function : IProgramItem
    {
        public Identifier Name { get; }

        public List<Identifier> Parameters { get; }

        public Block Body { get; }

        public Function(
            Identifier name,
            List<Identifier> parameters,
            Block body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
}
