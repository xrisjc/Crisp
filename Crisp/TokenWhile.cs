namespace Crisp
{
    class TokenWhile : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var guard = parser.ParseExpression();
            var body = parser.ParseExpression();
            return new ExpressionWhile(guard, body);
        }
    }
}
