using Crisp.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Eval
{
    class ObjFn : IObj
    {
        IExpression body;
        List<string> parameters;
        Environment environment;

        public int Arity => parameters.Count;

        public ObjFn(IExpression body, List<string> parameters, Environment environment)
        {
            this.body = body;
            this.parameters = parameters;
            this.environment = environment;
        }

        public ObjFn(Function function, Environment environment)
            : this(function.Body, function.Parameters.Select(p => p.Name).ToList(), environment)
        {
        }
        
        public IObj Call(List<IObj> arguments)
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
