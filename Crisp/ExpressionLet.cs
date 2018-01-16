namespace Crisp
{
    class ExpressionLet : IExpression
    {
        string name;
        IExpression value;

        public ExpressionLet(string name, IExpression value)
        {
            this.name = name;
            this.value = value;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objValue = value.Evaluate(environoment);
            environoment.Create(name, objValue);
            return objValue;
        }
    }
}
