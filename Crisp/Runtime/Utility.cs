﻿using Crisp.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Runtime
{
    static class Utility
    {
        public static IEnumerable<object> Evaluate(
            this IEnumerable<IExpression> expressions,
            Environment environment)
        {
            return from expression in expressions
                   select expression.Evaluate(environment);
        }

        public static IEnumerable<(dynamic, dynamic)> Evaluate(
            this IEnumerable<(IExpression, IExpression)> expressions,
            Environment environment)
        {
            return from e in expressions
                   select (e.Item1.Evaluate(environment), e.Item2.Evaluate(environment));
        }

        public static bool IsTrue(object x)
        {
            return !x.Equals(false) && !ReferenceEquals(x, Null.Instance);
        }

        public static bool CheckNumeric(dynamic x)
        {
            return x is int || x is double;
        }

        public static bool CheckNumeric(dynamic left, dynamic right)
        {
            return CheckNumeric(left) && CheckNumeric(right);
        }

        public static void Bind(List<string> names, List<object> values, Environment environment)
        {
            for (int i = 0; i < names.Count; i++)
            {
                environment.Create(names[i], values[i]);
            }
        }
    }
}
