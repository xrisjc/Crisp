using Crisp.Runtime;
using Crisp.Parsing;
using System.Collections.Generic;
using System.Linq;
using static Crisp.Runtime.Utility;

namespace Crisp.Ast
{
    class Call : IExpression
    {
        public Position Position { get; }

        List<IExpression> argumentExpressions;

        public IExpression FunctionExpression { get; }

        public IEnumerable<IExpression> ArgumentExpressions => argumentExpressions;

        public int Arity => argumentExpressions.Count;

        public Call(Position position, IExpression functionExpression, List<IExpression> argumentExpressions)
        {
            Position = position;
            FunctionExpression = functionExpression;
            this.argumentExpressions = argumentExpressions;
        }

        public Call(Position position, IExpression functionExpression)
            : this(position, functionExpression, new List<IExpression>())
        {
        }

        public object Evaluate(Environment environment)
        {
            var function = FunctionExpression.Evaluate(environment) as Runtime.Function;
            if (function == null)
            {
                throw new RuntimeErrorException(
                    Position,
                    "function call attempted on non function value");
            }

            if (function.Parameters.Count != Arity)
            {
                throw new RuntimeErrorException(
                    Position,
                    "function arity mismatch");
            }
            var arguments = ArgumentExpressions.Evaluate(environment).ToList();

            var localEnvironment = new Environment(function.Environment);
            Bind(function.Parameters, arguments, localEnvironment);

            return function.Body.Evaluate(localEnvironment);
        }
    }
}
