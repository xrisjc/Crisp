namespace Crisp
{
    class NullExpression : IExpression
    {
        private NullExpression()
        {
        }

        public IObject Evaluate(Environment environment)
        {
            return ObjectNull.Instance;
        }

        public static NullExpression Instance { get; } = new NullExpression();
    }
}
