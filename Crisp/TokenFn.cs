using System.Collections.Generic;

namespace Crisp
{
    class TokenFn : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var parameters = new List<string>();
            parser.Expect<TokenLParen>();
            if (parser.Match<TokenIdentifier>(out var identifier))
            {
                parameters.Add(identifier.Name);
                while (parser.Match<TokenComma>())
                {
                    identifier = parser.Expect<TokenIdentifier>();
                    parameters.Add(identifier.Name);
                }
            }
            parser.Expect<TokenRParen>();
            parser.Expect<TokenLBrace>();
            var body = parser.Parse();
            parser.Expect<TokenRBrace>();
            return new ExpressionFunction(body, parameters);
        }
    }
}
