using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Call : IExpression
    {
        public IExpression FunctionExpression { get; }

        public IEnumerable<IExpression> ArgumentExpressions { get; }

        public Call(IExpression functionExpression, IEnumerable<IExpression> argumentExpressions)
        {
            FunctionExpression = functionExpression;
            ArgumentExpressions = argumentExpressions;
        }

        public Call(IExpression functionExpression)
            : this(functionExpression, Enumerable.Empty<IExpression>())
        {
        }
    }
}
