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

        public IObj Evaluate(Environment environoment)
        {
            var objLeft = Left.Evaluate(environoment);
            var objRight = Right.Evaluate(environoment);

            if (ReferenceEquals(objLeft, objRight))
            {
                return Obj.True;
            }

            if ((objLeft is Obj<double> intLeft) && (objRight is Obj<double> intRight))
            {
                return intLeft.Value == intRight.Value ? Obj.True : Obj.False;
            }

            if ((objLeft is Obj<bool> boolLeft) && (objRight is Obj<bool> boolRight))
            {
                return boolLeft.Value == boolRight.Value ? Obj.True : Obj.False;
            }

            return Obj.False;
        }

        public override string ToString()
        {
            return $"({Left} = {Right})";
        }
    }
}
