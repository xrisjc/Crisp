using Crisp.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Eval
{
    class Function
    {
        IExpression body;
        List<string> parameters;
        Environment environment;

        public int Arity => parameters.Count;

        public Function(IExpression body, List<string> parameters, Environment environment)
        {
            this.body = body;
            this.parameters = parameters;
            this.environment = environment;
        }

        public Function(Ast.Function function, Environment environment)
            : this(function.Body, function.Parameters.ToList(), environment)
        {
        }
        
        public dynamic Call(List<dynamic> arguments)
        {
            var localEnvironment = new Environment(environment);

            for (int i = 0; i < parameters.Count; i++)
            {
                localEnvironment.Create(parameters[i], arguments[i]);
            }

            return body.Evaluate(localEnvironment);
        }
    }
}
