namespace Crisp.Ast
{
    class LiteralDouble : IExpression
    {
        public double Value { get; }

        public LiteralDouble(double value)
        {
            Value = value;
        }
    }
}
