using Crisp.Parsing;

namespace Crisp.Ast
{
    class AttributeAssignment : IExpression
    {
        public Position Position { get; set; }

        public IExpression Entity { get; }

        public string Name { get; }

        public IExpression Value { get; }

        public AttributeAssignment(Position position, IExpression entity, string name, IExpression value)
        {
            Position = position;
            Entity = entity;
            Name = name;
            Value = value;
        }
    }
}
