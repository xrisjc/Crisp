namespace Crisp
{
    class ExpressionIdentifier : IExpression
    {
        public string Name { get; }

        public ExpressionIdentifier(string name)
        {
            Name = name;
        }

        public IObject Evaluate(Environment environoment)
        {
            return environoment.Get(Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
