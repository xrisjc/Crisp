using System.Collections.Generic;

namespace Crisp.Ast
{
    class Command : IExpression
    {
        public CommandTag Tag { get; }

        public IEnumerable<IExpression> ArgumentExpressions { get; }

        public Command(CommandTag tag, IEnumerable<IExpression> argumentExpressions)
        {
            Tag = tag;
            ArgumentExpressions = argumentExpressions;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
