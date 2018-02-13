namespace Crisp
{
    class NamedExpression
    {
        public string Name { get; }

        public IExpression Expression { get; }

        public NamedExpression(string name, IExpression expression)
        {
            Name = name;
            Expression = expression;
        }
    }
}
