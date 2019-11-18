namespace Crisp.Ast
{
    class Const : IExpression
    {
        public string Name { get; }

        public Const(string name)
        {
            Name = name;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
