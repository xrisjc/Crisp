﻿using System.Collections.Generic;

namespace Crisp
{
    class ObjFnNative : IObjFn
    {
        IExpression body;
        List<string> parameters;
        Environment environment;

        public ObjFnNative(IExpression body, List<string> parameters, Environment environment)
        {
            this.body = body;
            this.parameters = parameters;
            this.environment = environment;
        }

        public IObj Call(List<IObj> arguments)
        {
            if (parameters.Count != arguments.Count)
            {
                throw new RuntimeErrorException("number of arguments doesn't match number of parameters");
            }

            var localEnvironment = new Environment(environment);

            for (int i = 0; i < parameters.Count; i++)
            {
                localEnvironment.Create(parameters[i], arguments[i]);
            }

            return body.Evaluate(localEnvironment);
        }

        public string Print()
        {
            return "<fn>";
        }
    }
}
