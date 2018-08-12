using System.Collections.Generic;

namespace Crisp.Ast
{
    class Program
    {
        public Dictionary<string, Literal> Consts { get; set; }

        public Dictionary<string, Function> Fns { get; set; }

        public List<IExpression> Expressions { get; set; }
    }
}
