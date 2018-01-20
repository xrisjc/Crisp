namespace Crisp
{
    class ExpressionOperatorBinary : IExpression
    {
        IOperatorBinary op;
        IExpression left, right;

        public ExpressionOperatorBinary(IOperatorBinary op, IExpression left,
            IExpression right)
        {
            this.op = op;
            this.left = left;
            this.right = right;
        }

        public IObj Evaluate(Environment environment)
        {
            var objLeft = left.Evaluate(environment);
            var objRight = right.Evaluate(environment);
            var result = op.Evaluate(objLeft, objRight);
            return result;
        }
    }
}
