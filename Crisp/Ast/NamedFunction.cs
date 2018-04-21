using System.Collections.Generic;

namespace Crisp.Ast
{
    class NamedFunction : Function
    {
        public string Name { get; }

        public NamedFunction(string name, List<string> parameters, IExpression body)
            : base(parameters, body)
        {
            Name = name;
        }
    }
}
