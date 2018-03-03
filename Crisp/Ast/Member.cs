namespace Crisp.Ast
{
    class Member : IExpression
    {
        public IExpression Expression { get; }

        public Identifier MemberIdentifier { get; }

        public Member(IExpression expression, Identifier memberIdentifier)
        {
            Expression = expression;
            MemberIdentifier = memberIdentifier;
        }
    }
}
