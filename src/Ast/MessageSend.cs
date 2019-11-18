using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class MessageSend : IExpression
    {
        public Position Position { get; }

        public IExpression EntityExpr { get; }

        public string Name { get; }

        public List<IExpression> ArgumentExprs { get; }

        public MessageSend(Position position, IExpression entityExpr, string name, List<IExpression> argumentExprs)
        {
            Position = position;
            EntityExpr = entityExpr;
            Name = name;
            ArgumentExprs = argumentExprs;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
