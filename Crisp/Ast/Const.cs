namespace Crisp.Ast
{
    class Const : IExpression
    {
        public string Name { get; set; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
