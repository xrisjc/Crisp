using Crisp.Eval;

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

        public object Evaluate(Environment environment)
        {
            var obj = Expression.Evaluate(environment);
            switch (obj)
            {
                case RecordInstance ri:
                    if (ri.MemberGet(Name, out var value))
                    {
                        return value;
                    }
                    else
                    {
                        throw new RuntimeErrorException($"cannot get member {Name}");
                    }

                default:
                    throw new RuntimeErrorException("object doesn't support member getting");
            }
        }
    }
}
