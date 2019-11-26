namespace Crisp.Ast
{
    class Literal : IExpression
    {
        public static Literal True { get; } = new Literal(true);

        public static Literal False { get; } = new Literal(false);

        public static Literal Null { get; } = new Literal(null);

        public object Value { get; }

        public Literal(object value)
        {
            Value = value;
        }
    }
}
