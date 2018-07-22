﻿namespace Crisp.Ast
{
    class LiteralNull : IExpression
    {
        private LiteralNull() { }

        public static LiteralNull Instance { get; } = new LiteralNull();

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
