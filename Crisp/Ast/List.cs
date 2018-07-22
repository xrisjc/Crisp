using System.Collections.Generic;

namespace Crisp.Ast
{
    class List : IExpression
    {
        public List<IExpression> Initializers { get; }

        public List(List<IExpression> initializers)
        {
            Initializers = initializers;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
