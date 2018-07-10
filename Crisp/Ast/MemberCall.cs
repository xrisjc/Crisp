using Crisp.Eval;
using Crisp.Parsing;
using System.Collections.Generic;
using System.Linq;

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

        public object Evaluate(Environment environment)
        {
            var record = Member.Expression.Evaluate(environment) as RecordInstance;
            if (record == null)
            {
                throw new RuntimeErrorException(
                    Position,
                    "method call must be on a record instance");
            }

            var function = record.GetMemberFunction(Member.Name);
            if (function.Parameters.Count != Arity + 1) // + 1 for "this" argument
            {
                throw new RuntimeErrorException(
                    Position,
                    "method call arity mismatch");
            }
            var arguments = ArgumentExpressions.Evaluate(environment).ToList();
            arguments.Add(record); // the "this" argument is at the end

            var localEnvironment = new Environment(function.Environment);
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                localEnvironment.Create(function.Parameters[i], arguments[i]);
            }

            return function.Body.Evaluate(localEnvironment);
        }
    }
}
