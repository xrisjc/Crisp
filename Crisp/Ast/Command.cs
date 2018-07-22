using System.Collections.Generic;

namespace Crisp.Ast
{
    class Command : IExpression
    {
        public CommandType Type { get; }

        public IEnumerable<IExpression> ArgumentExpressions { get; }

        public Command(CommandType type, IEnumerable<IExpression> argumentExpressions)
        {
            Type = type;
            ArgumentExpressions = argumentExpressions;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
