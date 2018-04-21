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
    }
}
