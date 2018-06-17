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
        
        public void Call(Stack<dynamic> stack)
        {
            var localEnvironment = new Environment(environment);

            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                localEnvironment.Create(parameters[i], stack.Pop());
            }

            body.Evaluate(stack, localEnvironment);
        }
    }
}
