using System.Collections.Generic;

namespace Crisp
{
    class TokenFn : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var parameters = new List<string>();
            string name = parser.Match<TokenIdentifier>(out var nameToken) ? nameToken.Name : null;

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
            var body = parser.ParseExpression();
            return new ExpressionFunction(name, parameters, body);
        }
    }
}
