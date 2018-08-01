using System.Collections.Generic;

namespace Crisp.Ast
{
    class Function : IExpression
    {
        public string Name { get; }

        public List<string> Parameters { get; }

        public List<IExpression> Body { get; }

        public Function(string name, List<string> parameters, List<IExpression> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
