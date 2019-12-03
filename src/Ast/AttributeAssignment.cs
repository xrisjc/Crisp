namespace Crisp.Ast
{
    class AttributeAssignment : IExpression
    {
        public IExpression Entity { get; }

        public Identifier Name { get; }

        public IExpression Value { get; }

        public AttributeAssignment(IExpression entity, Identifier name, IExpression value)
        {
            Entity = entity;
            Name = name;
            Value = value;
        }
    }
}
