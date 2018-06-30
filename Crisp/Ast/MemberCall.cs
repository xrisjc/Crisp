using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class MemberCall : IExpression
    {
        public Position Position { get; }

        private List<IExpression> argumentExpressions;

        public Member Member { get; }

        public List<IExpression> ArgumentExpressions => argumentExpressions;

        public int Arity => argumentExpressions.Count;

        public MemberCall(Position position, Member member, List<IExpression> argumentExpressions)
        {
            Position = position;
            Member = member;
            this.argumentExpressions = argumentExpressions;
        }
    }
}
