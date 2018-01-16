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

        public IObject Evaluate(Environment environoment)
        {
            var objLeft = Left.Evaluate(environoment);
            var objRight = Right.Evaluate(environoment);

            if ((objLeft is ObjectInteger intLeft) && (objRight is ObjectInteger intRight))
            {
                return new ObjectInteger(intLeft.Value + intRight.Value);
            }
            else
            {
                throw new RuntimeErrorException("can only add integer values");
            }
        }

        public override string ToString()
        {
            return $"({Left} + {Right})";
        }
    }
}
