namespace Crisp
{
    class ExpressionIdentifier : IExpression
    {
        public string Name { get; }

        public ExpressionIdentifier(string name)
        {
            Name = name;
        }

        public IObj Evaluate(Environment env) => env.Get(Name);
    }
}
