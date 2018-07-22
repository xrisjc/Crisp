﻿namespace Crisp.Ast
{
    class Branch : IExpression
    {
        public IExpression Condition { get; }

        public IExpression Consequence { get; }

        public IExpression Alternative { get; }

        public Branch(IExpression condition, IExpression consequence, IExpression alternative)
        {
            Condition = condition;
            Consequence = consequence;
            Alternative = alternative;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
