using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    class Call : IExpression
    {
        public Position Position { get; }

        List<IExpression> argumentExpressions;

        public IExpression FunctionExpression { get; }

        public IEnumerable<IExpression> ArgumentExpressions => argumentExpressions;

        public int Arity => argumentExpressions.Count;

        public Call(Position position, IExpression functionExpression, List<IExpression> argumentExpressions)
        {
            Position = position;
            FunctionExpression = functionExpression;
            this.argumentExpressions = argumentExpressions;
        }

        public Call(Position position, IExpression functionExpression)
            : this(position, functionExpression, new List<IExpression>())
        {
        }
    }
}
