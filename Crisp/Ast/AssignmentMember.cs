namespace Crisp.Ast
{
    class AssignmentMember : IExpression
    {
        public Member Target { get; }

        public IExpression Value { get; }

        public AssignmentMember(Member target, IExpression value)
        {
            Target = target;
            Value = value;
        }
    }
}
