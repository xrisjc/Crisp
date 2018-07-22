using System.Collections.Generic;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public List<string> Variables { get; }

        public Dictionary<string, Function> Functions { get; }

        public Record(List<string> variables, Dictionary<string, Function> functions)
        {
            Variables = variables;
            Functions = functions;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
