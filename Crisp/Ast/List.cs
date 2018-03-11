using System.Collections.Generic;

namespace Crisp.Ast
{
    class List : IExpression
    {
        public List<IExpression> Initializers { get; set; } = new List<IExpression>();
    }
}
