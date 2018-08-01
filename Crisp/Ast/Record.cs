using System.Collections.Generic;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public string Name { get; }

        public List<string> Variables { get; }

        public Dictionary<string, Function> Functions { get; }

        public Record(string name, List<string> variables, Dictionary<string, Function> functions)
        {
            Name = name;
            Variables = variables;
            Functions = functions;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
