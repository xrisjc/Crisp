﻿using System.Collections.Generic;

namespace Crisp
{
    class ExpressionFunction : IExpression
    {
        IExpression body;
        List<string> parameters;

        public ExpressionFunction(IExpression body, List<string> parameters)
        {
            this.body = body;
            this.parameters = parameters;
        }

        public IObj Evaluate(Environment environoment)
        {
            return new ObjFnNative(body, parameters, environoment);
        }
    }
}
