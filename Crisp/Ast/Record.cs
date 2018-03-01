using System.Collections.Generic;

namespace Crisp.Ast
{
    class Record : IExpression
    {
        public List<Identifier> Members { get; }

        public Record(List<Identifier> members)
        {
            Members = members;
        }

        public Record()
            : this(new List<Identifier>())
        {
        }
    }
}
