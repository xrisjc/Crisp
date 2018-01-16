namespace Crisp
{
    abstract class TokenInfixOperator : Token
    {
        public abstract IExpression CreateExpression(IExpression left, IExpression right);

        public override IExpression Led(Parser parser, IExpression left)
        {
            var right = parser.Parse(Lbp);
            return CreateExpression(left, right);
        }
    }
}
