namespace Crisp.Ast
{
    class This : IExpression
    {
        private This() { }

        public static This Instance { get; } = new This();

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
