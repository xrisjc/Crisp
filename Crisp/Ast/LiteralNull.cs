namespace Crisp.Ast
{
    class LiteralNull : IExpression
    {
        private LiteralNull() { }

        public static LiteralNull Instance { get; } = new LiteralNull();
    }
}
