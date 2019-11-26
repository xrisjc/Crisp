using System.Collections.Generic;

namespace Crisp.Ast
{
    class While : IExpression
    {
        public IExpression Guard { get; }

        public List<IExpression> Body { get; }

        public While(IExpression guard, List<IExpression> body)
        {
            Guard = guard;
            Body = body;
        }
    }
}
