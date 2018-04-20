using Crisp.Parsing;

namespace Crisp.Ast
{
    class OperatorBinary : IExpression
    {
        public Token Token { get; }

        public OperatorInfix Op { get; }

        public IExpression Left { get; }

        public IExpression Right { get; }

        public OperatorBinary(Token token, OperatorInfix op, IExpression left, IExpression right)
        {
            Token = token;
            Op = op;
            Left = left;
            Right = right;
        }
    }
}
