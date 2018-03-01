using System.Collections.Generic;

namespace Crisp.Ast
{
    class Block : IExpression
    {
        public List<IExpression> Body { get; }

        public Block(List<IExpression> body)
        {
            Body = body;
        }
    }
}
