namespace Crisp.Ast
{
    class AttributeAccess : IExpression
    {
        public IExpression Entity { get; }

        public Identifier Name { get; }

        public AttributeAccess(IExpression entity, Identifier name)
        {
            Entity = entity;
            Name = name;
        }
    }
}
