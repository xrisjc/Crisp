namespace Crisp
{
    abstract class TokenInfixOperator : Token
    {
        public abstract IExpression CreateExpression(IExpression left, IExpression right);

        public override IExpression Led(Parser parser, IExpression left)
        {
            var right = parser.ParseExpression(Lbp);
            return CreateExpression(left, right);
        }
    }
}
