﻿using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class AssignmentIndexing : IExpression
    {
        public Indexing Target { get; }

        public IExpression Value { get; }

        public AssignmentIndexing(Indexing target, IExpression value)
        {
            Target = target;
            Value = value;
        }

        public object Evaluate(Environment environment)
        {
            var target = Target.Indexable.Evaluate(environment);
            var index = Target.Index.Evaluate(environment);
            var value = Value.Evaluate(environment);
            switch (target)
            {
                case List<dynamic> l when index is int i:
                    if (i < 0 || i >= l.Count)
                    {
                        throw new RuntimeErrorException("Index out of bounds of list.");
                    }
                    l[i] = value;
                    break;

                case List<dynamic> l:
                    throw new RuntimeErrorException("Lists must be indexed by integers.");

                case Dictionary<dynamic, dynamic> d:
                    d[index] = value;
                    break;

                default:
                    throw new RuntimeErrorException("Set index on non-indexable object indexed.");
            }
            return value;
        }
    }
}
