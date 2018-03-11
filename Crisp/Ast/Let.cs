namespace Crisp.Ast
{
    class Let : IExpression
    {
        public Identifier Identifier { get; set; }
        public IExpression Value { get; set; }
    }
}
