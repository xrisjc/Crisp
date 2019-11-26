using System.Collections.Generic;

namespace Crisp.Ast
{
    class Write : IExpression
    {
        public List<IExpression> Arguments { get; set; }
    }
}
