namespace Crisp.Ast
{
    class Var : IExpression
    {
        public string Name { get; }

        public IExpression InitialValue { get; }

        public Var(string name, IExpression initialValue)
        {
            Name = name;
            InitialValue = initialValue;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
