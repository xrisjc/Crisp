using System.Collections.Generic;

namespace Crisp
{
    class ExpressionFunction : IExpression
    {
        string name;
        IExpression body;
        List<string> parameters;

        public ExpressionFunction(string name, List<string> parameters, IExpression body)
        {
            this.name = name;
            this.body = body;
            this.parameters = parameters;
        }

        public IObj Evaluate(Environment environoment)
        {
            var function = new ObjFnNative(body, parameters, environoment);
            if (name != null)
            {
                environoment.Create(name, function);
            }
            return function;
        }
    }
}
