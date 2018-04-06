using System.Collections.Generic;

namespace Crisp.Ast
{
    class MemberCall : IExpression
    {
        public Member Member { get; }

        public List<IExpression> ArgumentExpressions { get; }

        public MemberCall(Member member, List<IExpression> argumentExpressions)
        {
            Member = member;
            ArgumentExpressions = argumentExpressions;
        }
    }
}
