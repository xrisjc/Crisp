using Crisp.Runtime;
using Crisp.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class MemberCall : IExpression
    {
        public Position Position { get; }

        private List<IExpression> argumentExpressions;

        public AttributeAccess Member { get; }

        public List<IExpression> ArgumentExpressions => argumentExpressions;

        public int Arity => argumentExpressions.Count;

        public MemberCall(Position position, AttributeAccess member, List<IExpression> argumentExpressions)
        {
            Position = position;
            Member = member;
            this.argumentExpressions = argumentExpressions;
        }

        public object Evaluate(Environment environment)
        {
            var entity = Member.Entity.Evaluate(environment) as IEntity;
            if (entity == null)
            {
                throw new RuntimeErrorException(
                    Position,
                    "method call must be on a record instance");
            }

            if (!entity.GetMethod(Member.Name, out var method))
            {
                throw new RuntimeErrorException($"no method named ${Member.Name}");
            }
            if (method.Parameters.Count != Arity + 1) // + 1 for "this" argument
            {
                throw new RuntimeErrorException(
                    Position,
                    "method call arity mismatch");
            }
            var arguments = ArgumentExpressions.Evaluate(environment).ToList();
            arguments.Add(entity); // the "this" argument is at the end

            var localEnvironment = new Environment(method.Environment);
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                localEnvironment.Create(method.Parameters[i], arguments[i]);
            }

            return method.Body.Evaluate(localEnvironment);
        }
    }
}
