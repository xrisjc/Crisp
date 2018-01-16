namespace Crisp
{
    class ExpressionAdd : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public ExpressionAdd(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objLeft = Left.Evaluate(environoment);
            var objRight = Right.Evaluate(environoment);

            if ((objLeft is Obj<double> numRight) && (objRight is Obj<double> numLeft))
            {
                return Obj.Create(numRight.Value + numLeft.Value);
            }
            else
            {
                throw new RuntimeErrorException("can only add integer values");
            }
        }
    }
}
