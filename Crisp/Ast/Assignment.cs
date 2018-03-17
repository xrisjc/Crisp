namespace Crisp.Ast
{
    class Assignment<T> : IExpression
    {
        public T Target { get; }

        public IExpression Value { get; }

        public Assignment(T target, IExpression value)
        {
            Target = target;
            Value = value;
        }
    }
}
