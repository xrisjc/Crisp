using System.Collections.Generic;

namespace Crisp.Ast
{
    class Map : IExpression
    {
        public IEnumerable<(IExpression, IExpression)> Initializers { get; }

        public Map(IEnumerable<(IExpression, IExpression)> initializers)
        {
            Initializers = initializers;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
