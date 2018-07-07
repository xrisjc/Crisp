namespace Crisp.Ast
{
    class Member : IExpression
    {
        public IExpression Expression { get; }

        public string Name { get; }

        public Member(IExpression expression, string name)
        {
            Expression = expression;
            Name = name;
        }
    }
}
