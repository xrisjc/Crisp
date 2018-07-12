using Crisp.Runtime;

namespace Crisp.Ast
{
    class For : IExpression
    {
        public string VariableName { get; }

        public IExpression Start { get; }

        public IExpression End { get; }

        public IExpression Body { get; }

        public For(string variableName, IExpression start, IExpression end, IExpression body)
        {
            VariableName = variableName;
            Start = start;
            End = end;
            Body = body;
        }

        public object Evaluate(Environment environment)
        {
            dynamic start = Start.Evaluate(environment);
            Runtime.Utility.CheckNumeric(start);
            dynamic end = End.Evaluate(environment);
            Runtime.Utility.CheckNumeric(end);
            for (var i = start; i <= end; i = i + 1)
            {
                var localEnvironment = new Environment(environment);
                localEnvironment.Create(VariableName, i);
                Body.Evaluate(localEnvironment);
            }
            return Null.Instance;
        }
    }
}
