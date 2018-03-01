using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class Call : IExpression
    {
        public IExpression FunctionExpression { get; }

        public List<IExpression> ArgumentExpressions { get; }

        public Call(IExpression functionExpression, List<IExpression> argumentExpressions)
        {
            FunctionExpression = functionExpression;
            ArgumentExpressions = argumentExpressions;
        }

        public Call(IExpression functionExpression)
            : this(functionExpression, new List<IExpression>())
        {
        }
    }
}
