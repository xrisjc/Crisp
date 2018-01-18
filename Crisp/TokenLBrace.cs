using System.Collections.Generic;

namespace Crisp
{
    class TokenLBrace : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var body = new List<IExpression>();
            while (!parser.Match<TokenRBrace>())
            {
                var expression = parser.ParseExpression();
                body.Add(expression);
            }
            return new ExpressionBlock(body);
        }
    }
}
