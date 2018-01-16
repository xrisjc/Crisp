namespace Crisp
{
    class ExpressionIdentifier : IExpression
    {
        public string Name { get; }

        public ExpressionIdentifier(string name)
        {
            Name = name;
        }

        public IObj Evaluate(Environment environoment)
        {
            return environoment.Get(Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
