using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class Function
    {
        IExpression body;
        List<string> parameters;
        Environment environment;

        public int Arity => parameters.Count;

        public Function(List<string> parameters, IExpression body, Environment environment)
        {
            this.parameters = parameters;
            this.body = body;
            this.environment = environment;
        }
        
        public void Call(Stack<dynamic> stack, List<dynamic> arguments)
        {
            var localEnvironment = new Environment(environment);

            for (int i = 0; i < parameters.Count; i++)
            {
                localEnvironment.Create(parameters[i], arguments[i]);
            }

            body.Evaluate(stack, localEnvironment);
        }
    }
}
