using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class Call : IExpression
    {
        public Position Position { get; set; }

        public string Name { get; set; }

        public List<IExpression> Arguments { get; set; }

        public int Arity => Arguments.Count;

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
