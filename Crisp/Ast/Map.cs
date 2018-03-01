using System.Collections.Generic;

namespace Crisp.Ast
{
    class Map : IExpression
    {
        public List<(IExpression, IExpression)> Initializers { get; }

        public Map(List<(IExpression, IExpression)> initializers)
        {
            Initializers = initializers;
        }

        public Map()
            : this(new List<(IExpression, IExpression)>())
        {
        }
    }
}
