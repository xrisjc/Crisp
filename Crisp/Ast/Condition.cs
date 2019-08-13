﻿using System.Collections.Generic;

namespace Crisp.Ast
{
    class Condition : IExpression
    {
        public List<Branch> Branches { get; }

        public Condition(List<Branch> branches)
        {
            Branches = branches;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
