using System.Collections.Generic;

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

        public IObject Evaluate(Environment environoment)
        {
            return new ObjectFunctionNative(body, parameters, environoment);
        }

        public override string ToString()
        {
            return $"fn ({string.Join(", ", parameters)}) {{ {body} }}";
        }
    }
}
