using Crisp.Eval;

namespace Crisp.Ast
{
    class AssignmentIdentifier : IExpression
    {
        public Identifier Target { get; }

        public IExpression Value { get; }

        public AssignmentIdentifier(Identifier target, IExpression value)
        {
            Target = target;
            Value = value;
        }

        public object Evaluate(Environment environment)
        {
            var value = Value.Evaluate(environment);
            if (environment.Set(Target.Name, value))
            {
                return value;
            }
            else
            {
                throw new RuntimeErrorException(
                    Target.Position,
                    $"Cannot assign value to unbound name <{Target.Name}>");
            }
        }
    }
}
