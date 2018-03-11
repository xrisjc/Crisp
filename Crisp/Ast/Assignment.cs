namespace Crisp.Ast
{
    class Assignment<T> : IExpression
    {
        public T Target { get; set; }

        public IExpression Value { get; set; }
    }
}
