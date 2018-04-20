using Crisp.Parsing;

namespace Crisp.Ast
{
    class OperatorUnary : IExpression
    {
        public Token Token { get; }

        public OperatorPrefix Op { get; }

        public IExpression Expression { get; }

        public OperatorUnary(Token token, OperatorPrefix op, IExpression expression)
        {
            Token = token;
            Op = op;
            Expression = expression;
        }
    }
}
