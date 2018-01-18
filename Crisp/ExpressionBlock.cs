using System.Collections.Generic;

namespace Crisp
{
    class ExpressionBlock : IExpression
    {
        IEnumerable<IExpression> body;

        public ExpressionBlock(IEnumerable<IExpression> body)
        {
            this.body = body;
        }

        public IObj Evaluate(Environment environment)
        {
            var localEnvironment = new Environment(environment);
            IObj result = Obj.Null;
            foreach (var expression in body)
            {
                result = expression.Evaluate(localEnvironment);
            }
            return result;
        }
    }
}
