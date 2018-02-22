using System.Collections.Generic;

namespace Crisp.Ast
{
    class NamedFunction : Function
    {
        public Identifier Name { get; }

        public NamedFunction(Identifier name, IEnumerable<Identifier> parameters, IExpression body)       
            : base(parameters, body)
        {
            Name = name;
        }
    }
}
