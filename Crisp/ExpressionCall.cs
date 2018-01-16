using System.Collections.Generic;
using System.Linq;

namespace Crisp
{
    class ExpressionCall : IExpression
    {
        IExpression functionExpression;
        List<IExpression> argumentExpressions;

        public ExpressionCall(IExpression functionExpression, List<IExpression> argumentExpressions)
        {
            this.functionExpression = functionExpression;
            this.argumentExpressions = argumentExpressions;
        }

        public IObj Evaluate(Environment environment)
        {
            if (functionExpression.Evaluate(environment) is IObjFn function)
            {
                var arguments = argumentExpressions.Select(arg => arg.Evaluate(environment))
                                                   .ToList();
                return function.Call(arguments);
            }
            else
            {
                throw new RuntimeErrorException("function call attempted on non function value");
            }
        }
    }
}
