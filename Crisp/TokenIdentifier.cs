namespace Crisp
{
    class TokenIdentifier : Token
    {
        public string Name { get; }

        public TokenIdentifier(string name)
        {
            Name = name;
        }

        public override IExpression Nud(Parser parser)
        {
            return new ExpressionIdentifier(Name);
        }

        public override string ToString()
        {
            return $"IDENTIFIER({Name})";
        }
    }
}
