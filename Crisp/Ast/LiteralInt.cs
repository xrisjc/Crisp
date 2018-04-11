namespace Crisp.Ast
{
    class LiteralInt : IExpression
    {
        public int Value { get; }

        public LiteralInt(int value)
        {
            Value = value;
        }
    }
}
