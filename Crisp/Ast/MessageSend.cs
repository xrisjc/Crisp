using Crisp.Runtime;
using Crisp.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Ast
{
    class MessageSend : IExpression
    {
        Position position;
        IExpression entityExpr;
        string name;
        List<IExpression> argumentExprs;

        public MessageSend(Position position, IExpression entityExpr,
            string name, List<IExpression> argumentExprs)
        {
            this.position = position;
            this.entityExpr = entityExpr;
            this.name = name;
            this.argumentExprs = argumentExprs;
        }

        public object Evaluate(Environment environment)
        {
            var entity = entityExpr.Evaluate(environment) as Entity;
            if (entity == null)
            {
                throw new RuntimeErrorException(
                    position,
                    "message sent to non entity object");
            }

            var arguments = argumentExprs.Select(e => e.Evaluate(environment)).ToList();
            var messageHandled = entity.SendMessage(name, arguments, out var value);
            if (!messageHandled)
            {
                throw new RuntimeErrorException(
                    position,
                    $"error sending message '{name}'");
            }
            return value;
        }
    }
}
