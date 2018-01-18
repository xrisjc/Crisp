namespace Crisp
{
    class ExpressionBranch : IExpression
    {
        IExpression condition, consequence, alternative;

        public ExpressionBranch(IExpression condition, IExpression consequence,
            IExpression alternative)
        {
            this.condition = condition;
            this.consequence = consequence;
            this.alternative = alternative;
        }

        public IObj Evaluate(Environment environment)
        {
            var objResult = condition.Evaluate(environment);
            if (objResult is Obj<bool> boolResult)
            {
                var expression = boolResult.Value
                    ? consequence
                    : alternative;
                return expression.Evaluate(environment);
            }

            throw new RuntimeErrorException(
                "an if condition must be a bool value");
        }
    }
}
