namespace Crisp.Ast
{
    class Literal<T> : IExpression
    {
        public T Value { get; }

        public Literal(T value)
        {
            Value = value;
        }
    }
}
