namespace Crisp
{
    class ExpressionEquals : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public ExpressionEquals(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public IObject Evaluate(Environment environoment)
        {
            var objLeft = Left.Evaluate(environoment);
            var objRight = Right.Evaluate(environoment);

            if (ReferenceEquals(objLeft, objRight))
            {
                return new ObjectBoolean(true);
            }

            if ((objLeft is ObjectInteger intLeft) && (objRight is ObjectInteger intRight))
            {
                return new ObjectBoolean(intLeft.Value == intRight.Value);
            }

            if ((objLeft is ObjectBoolean boolLeft) && (objRight is ObjectBoolean boolRight))
            {
                return new ObjectBoolean(boolLeft.Value == boolRight.Value);
            }

            return new ObjectBoolean(false);
        }

        public override string ToString()
        {
            return $"({Left} = {Right})";
        }
    }
}
