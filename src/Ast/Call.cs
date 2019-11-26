using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class Call : IExpression
    {
        public Position Position { get; }

        public string Name { get; }

        public List<IExpression> Arguments { get; }

        public Call(Position position, string name, List<IExpression> arguments)
        {
            Position = position;
            Name = name;
            Arguments = arguments;
        }
    }
}
