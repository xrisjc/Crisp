namespace Crisp
{
    class Token
    {
        public virtual Precidence Lbp { get; } = Precidence.Lowest;

        public virtual IExpression Nud(Parser parser)
        {
            throw new SyntaxErrorException($"unexpected token '{this}'");
        }

        public virtual IExpression Led(Parser parser, IExpression left)
        {
            throw new SyntaxErrorException($"unexpected token '{this}'");
        }
    }
}
