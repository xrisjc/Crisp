using System.Collections.Generic;
using Crisp.Runtime;

namespace Crisp.Ast
{
    class Map : IExpression
    {
        public IEnumerable<(IExpression, IExpression)> Initializers { get; }

        public Map(IEnumerable<(IExpression, IExpression)> initializers)
        {
            Initializers = initializers;
        }

        public object Evaluate(Environment environment)
        {
            var map = new Dictionary<object, object>();
            foreach (var (keyExpr, valueExpr) in Initializers)
            {
                var key = keyExpr.Evaluate(environment);
                var value = valueExpr.Evaluate(environment);
                map[key] = value;
            }
            return new Runtime.Map(map);
        }
    }
}
