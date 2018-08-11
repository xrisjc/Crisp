using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function : IExpression
    {
        public string Name { get; set; }

        public List<string> Parameters { get; set; }

        public List<IExpression> Body { get; set; }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
