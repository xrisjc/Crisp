namespace Crisp
{
    class ExpressionLet : IExpression
    {
        public string Name { get; }

        public IExpression Value { get; }

        public ExpressionLet(string name, IExpression value)
        {
            Name = name;
            Value = value;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objValue = Value.Evaluate(environoment);
            environoment.Create(Name, objValue);
            return objValue;
        }

        public override string ToString()
        {
            return $"let {Name} := {Value}";
        }
    }
}
