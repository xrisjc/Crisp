using Crisp.Eval;
using System.Collections.Generic;
using static Crisp.Eval.Utility;

namespace Crisp.Ast
{
    class Indexing : IExpression
    {
        public IExpression Indexable { get; }

        public IExpression Index { get; }

        public Indexing(IExpression indexable, IExpression index)
        {
            Indexable = indexable;
            Index = index;
        }

        public object Evaluate(Environment environment)
        {
            var target = Indexable.Evaluate(environment);
            var index = Index.Evaluate(environment);

            switch (target)
            {
                case string s when index is int i:
                    if (i < 0 || i >= s.Length)
                    {
                        throw new RuntimeErrorException("Index out of bounds of string.");
                    }
                    return s[i].ToString();

                case string s:
                    throw new RuntimeErrorException("Strings must be indexed by integers.");

                case List<dynamic> l when index is int i:
                    if (i < 0 || i >= l.Count)
                    {
                        throw new RuntimeErrorException("Index out of bounds of list.");
                    }
                    return l[i];

                case List<dynamic> l:
                    throw new RuntimeErrorException("Lists must be indexed by integers.");

                case Dictionary<dynamic, dynamic> d:
                    if (!d.ContainsKey(index))
                    {
                        throw new RuntimeErrorException("Key not found in map.");
                    }
                    return d[index];

                default:
                    throw new RuntimeErrorException("Get index on non-indexable object indexed.");
            }
        }
    }
}
