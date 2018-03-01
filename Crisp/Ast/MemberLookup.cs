namespace Crisp.Ast
{
    class MemberLookup : IExpression
    {
        public IExpression Expression { get; }

        public Identifier MemberIdentifier { get; }

        public MemberLookup(IExpression expression, Identifier memberIdentifier)
        {
            Expression = expression;
            MemberIdentifier = memberIdentifier;
        }
    }
}
