namespace Crisp.Ast
{
    class LiteralString : IExpression
    {
        public string Value { get; }

        public LiteralString(string value)
        {
            Value = value;
        }
    }
}
