using System.Collections.Generic;

namespace Crisp.Ast
{
    class MemberFunction : NamedFunction
    {
        public Identifier Record { get; }

        public MemberFunction(Identifier record, Identifier name, List<Identifier> parameters,
            IExpression body)
            : base(name, parameters, body)
        {
            Record = record;
        }
    }
}
