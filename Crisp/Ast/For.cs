using System.Collections.Generic;

namespace Crisp.Ast
{
    class For : IExpression
    {
        public string VariableName { get; }

        public IExpression Start { get; }

        public IExpression End { get; }

        public List<IExpression> Body { get; }

        public For(string variableName, IExpression start, IExpression end, List<IExpression> body)
        {
            VariableName = variableName;
            Start = start;
            End = end;
            Body = body;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
