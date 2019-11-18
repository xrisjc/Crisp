using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function
    {
        public Identifier Name { get; set; }

        public List<string> Parameters { get; set; }

        public List<IExpression> Body { get; set; }
    }
}
