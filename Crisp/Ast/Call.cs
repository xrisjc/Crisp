using System.Collections.Generic;

namespace Crisp.Ast
{
    class Call : IExpression
    {
        List<IExpression> argumentExpressions;

        public IExpression FunctionExpression { get; }

        public IEnumerable<IExpression> ArgumentExpressions => argumentExpressions;

        public int Arity => argumentExpressions.Count;

        public Call(IExpression functionExpression, List<IExpression> argumentExpressions)
        {
            FunctionExpression = functionExpression;
            this.argumentExpressions = argumentExpressions;
        }

        public Call(IExpression functionExpression)
            : this(functionExpression, new List<IExpression>())
        {
        }
    }
}
