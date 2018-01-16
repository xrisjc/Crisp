namespace Crisp
{
    class ExpressionMultiply : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public ExpressionMultiply(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public IObj Evaluate(Environment environoment)
        {
            var objLeft = Left.Evaluate(environoment);
            var objRight = Right.Evaluate(environoment);

            if ((objLeft is Obj<double> intLeft) && (objRight is Obj<double> intRight))
            {
                return Obj.Create(intLeft.Value * intRight.Value);
            }
            else
            {
                throw new RuntimeErrorException("can only multiply numbers");
            }
        }

        public override string ToString()
        {
            return $"({Left} * {Right})";
        }
    }
}
