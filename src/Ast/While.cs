namespace Crisp.Ast
{
    class While : IExpression
    {
        public IExpression Guard { get; }

        public Block Body { get; }

        public While(IExpression guard, Block body)
        {
            Guard = guard;
            Body = body;
        }
    }
}
