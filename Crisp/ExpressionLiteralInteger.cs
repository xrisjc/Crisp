namespace Crisp
{
    class ExpressionLiteralInteger : IExpression
    {
        public long Value { get; }

        public ExpressionLiteralInteger(long value)
        {
            Value = value;
        }

        public IObject Evaluate(Environment environoment)
        {
            return new ObjectInteger(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
