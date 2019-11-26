namespace Crisp.Ast
{
    class AttributeAccess : IExpression
    {
        public IExpression Entity { get; }

        public string Name { get; }

        public AttributeAccess(IExpression entity, string name)
        {
            Entity = entity;
            Name = name;
        }
    }
}
