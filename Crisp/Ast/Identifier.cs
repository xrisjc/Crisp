using Crisp.Runtime;
using Crisp.Parsing;

namespace Crisp.Ast
{
    class Identifier : IExpression
    {
        public Position Position { get; }

        public string Name { get; }

        public Identifier(Position position, string name)
        {
            Position = position;
            Name = name;
        }

        public object Evaluate(Environment environment)
        {
            if (environment.Get(Name, out var value))
            {
                return value;
            }
            else
            {
                throw new RuntimeErrorException(
                    Position,
                    $"<{Name}> not bound to a value.");
            }
        }
    }
}
