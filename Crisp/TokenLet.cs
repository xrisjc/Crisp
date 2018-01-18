namespace Crisp
{
    class TokenLet : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var identifier = parser.Expect<TokenIdentifier>();
            parser.Expect<TokenAssignment>();
            var value = parser.ParseExpression(Precidence.Assignment);
            return new ExpressionLet(identifier.Name, value);
        }
    }
}
