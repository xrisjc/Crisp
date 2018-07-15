using Crisp.Runtime;
using Crisp.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class MemberCall : IExpression
    {
        Position position;
        List<IExpression> argumentExpressions;

        public AttributeAccess Member { get; }

        public int Arity => argumentExpressions.Count;

        public MemberCall(Position position, AttributeAccess member, List<IExpression> argumentExpressions)
        {
            this.position = position;
            Member = member;
            this.argumentExpressions = argumentExpressions;
        }

        public object Evaluate(Environment environment)
        {
            var entity = Member.Entity.Evaluate(environment) as IEntity;
            if (entity == null)
            {
                throw new RuntimeErrorException(
                    position,
                    "message sent to non entity object");
            }

            var arguments = argumentExpressions.Select(e => e.Evaluate(environment)).ToList();
            var messageHandled = entity.SendMessage(Member.Name, arguments, out var value);
            if (!messageHandled)
            {
                throw new RuntimeErrorException(
                    position,
                    $"error sending message '{Member.Name}'");
            }
            return value;
        }
    }
}
