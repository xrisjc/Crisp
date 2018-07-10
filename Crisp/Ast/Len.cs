﻿using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class Len : IExpression
    {
        public IExpression Expression { get; }

        public Len(IExpression expression)
        {
            Expression = expression;
        }

        public object Evaluate(Environment environment)
        {
            dynamic obj = Expression.Evaluate(environment);
            if (obj is Dictionary<dynamic, dynamic> || obj is List<dynamic>)
            {
                return obj.Count;
            }
            else
            {
                throw new RuntimeErrorException("unsupported object passed to len()");
            }
        }
    }
}
