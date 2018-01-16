namespace Crisp
{
    class TokenAssignment : Token
    {
        public override Precidence Lbp => Precidence.Assignment;

        public override IExpression Led(Parser parser, IExpression left)
        {
            if (left is ExpressionIdentifier identifier)
            {
                var right = parser.Parse(Lbp);
                return new ExpressionAssignment(identifier.Name, right);
            }
            else
            {
                throw new SyntaxErrorException("left hand side of assignment must be assignable");
            }
        }
    }
}
