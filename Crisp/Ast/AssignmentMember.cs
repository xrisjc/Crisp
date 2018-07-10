using Crisp.Eval;

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

        public object Evaluate(Environment environment)
        {
            var obj = Target.Expression.Evaluate(environment);
            var value = Value.Evaluate(environment);
            switch (obj)
            {
                case RecordInstance ri:
                    if (ri.MemberSet(Target.Name, value) == false)
                    {
                        throw new RuntimeErrorException($"Member {Target.Name} not found.");
                    }
                    break;

                default:
                    throw new RuntimeErrorException("object doesn't support member setting");
            }
            return value;
        }
    }
}
