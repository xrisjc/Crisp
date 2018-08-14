namespace Crisp.Ast
{
    class RecordConstructor : IExpression
    {
        public Identifier RecordName { get; set; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
