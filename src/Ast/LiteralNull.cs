namespace Crisp.Ast
{
    class LiteralNull : IExpression
    {
        public static LiteralNull Instance { get; } = new LiteralNull();

        private LiteralNull()
        {
        }
    }
}
