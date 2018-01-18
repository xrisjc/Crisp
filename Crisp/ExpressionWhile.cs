namespace Crisp
{
    class ExpressionWhile : IExpression
    {
        IExpression guard;
        IExpression body;

        public ExpressionWhile(IExpression guard, IExpression body)
        {
            this.guard = guard;
            this.body = body;
        }

        public IObj Evaluate(Environment environment)
        {
            while (true)
            {
                var predicate = guard.Evaluate(environment);
                if (predicate is Obj<bool> boolPredicate)
                {
                    if (boolPredicate.Value == false)
                    {
                        return Obj.Null;
                    }
                }
                else
                {
                    throw new RuntimeErrorException("a while guard must be a bool value");
                }
                body.Evaluate(environment);
            }
        }
    }
}
