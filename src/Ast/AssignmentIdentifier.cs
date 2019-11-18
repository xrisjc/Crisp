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

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
