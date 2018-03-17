using System.Collections.Generic;

namespace Crisp.Ast
{
    class Block : IExpression
    {
        public IEnumerable<IExpression> Body { get; }

        public Block(IEnumerable<IExpression> body)
        {
            Body = body;
        }
    }
}
