namespace Crisp.Ast
{
    class While : IExpression
    {
        public IExpression Guard { get; }
        public IExpression Body { get; }

        public While(IExpression guard, IExpression body)
        {
            Guard = guard;
            Body = body;
        }
    }
}
