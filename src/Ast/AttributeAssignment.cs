namespace Crisp.Ast
{
    class AttributeAssignment : IExpression
    {
        public IExpression Entity { get; }

        public string Name { get; }

        public IExpression Value { get; }

        public AttributeAssignment(IExpression entity, string name, IExpression value)
        {
            Entity = entity;
            Name = name;
            Value = value;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
