namespace Crisp
{
    class ExpressionLiteralNull : IExpression
    {
        private ExpressionLiteralNull() { }

        public IObj Evaluate(Environment environment) => Obj.Null;

        public static ExpressionLiteralNull Instance { get; } =
            new ExpressionLiteralNull();
    }
}
