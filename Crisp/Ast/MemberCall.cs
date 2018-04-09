using System.Collections.Generic;

namespace Crisp.Ast
{
    class MemberCall : IExpression
    {
        List<IExpression> argumentExpressions;

        public Member Member { get; }

        public List<IExpression> ArgumentExpressions => argumentExpressions;

        public int Arity => argumentExpressions.Count;

        public MemberCall(Member member, List<IExpression> argumentExpressions)
        {
            Member = member;
            this.argumentExpressions = argumentExpressions;
        }
    }
}
