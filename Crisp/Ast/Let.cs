namespace Crisp.Ast
{
    class Let : IExpression
    {
        public Identifier Identifier { get; }

        public IExpression Value { get; }

        public Let(Identifier identifier, IExpression value)
        {
            Identifier = identifier;
            Value = value;
        }
    }
}
