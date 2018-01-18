using System.Collections.Generic;

namespace Crisp
{
    class TokenLParen : Token
    {
        public override Precidence Lbp => Precidence.Parentheses;

        public override IExpression Nud(Parser parser)
        {
            var expression = parser.ParseExpression();
            parser.Expect<TokenRParen>();
            return expression;
        }

        public override IExpression Led(Parser parser, IExpression left)
        {
            var argumentExpressions = new List<IExpression>();

            if (!parser.Match<TokenRParen>())
            {
                do
                {
                    argumentExpressions.Add(parser.ParseExpression());
                }
                while (parser.Match<TokenComma>());

                parser.Expect<TokenRParen>();
            }

            return new ExpressionCall(left, argumentExpressions);
        }
    }
}
