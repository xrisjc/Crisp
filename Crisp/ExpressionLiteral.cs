namespace Crisp
{
    class ExpressionLiteral<T> : IExpression
    {
        T value;

        public ExpressionLiteral(T value)
        {
            this.value = value;
        }

        public IObj Evaluate(Environment environoment)
        {
            return Obj.Create(value);
        }

        public override string ToString()
        {
            if (value is string strValue)
            {
                return $"'{value}'";
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
