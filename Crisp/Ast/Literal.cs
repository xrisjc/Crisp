namespace Crisp.Ast
{
    class Literal : IExpression
    {
        public object Value { get; set; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
