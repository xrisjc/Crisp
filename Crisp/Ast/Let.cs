using Crisp.Eval;

namespace Crisp.Ast
{
    class Let : IExpression
    {
        public Identifier Identifier { get; }

        public IExpression Value { get; }

        public Let(Identifier identifier, IExpression value)
        {
            Identifier = identifier;
            Value = value;
        }

        public object Evaluate(Environment environment)
        {
            var value = Value.Evaluate(environment);
            if (environment.Create(Identifier.Name, value))
            {
                return value;
            }
            else
            {
                throw new RuntimeErrorException(
                    Identifier.Position,
                    $"Name <{Identifier.Name}> was already bound previously.");
            }
        }
    }
}
