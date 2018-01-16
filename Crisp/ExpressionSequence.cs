namespace Crisp
{
    class ExpressionSequence : IExpression
    {
        public IExpression Left { get; }

        public IExpression Right { get; }

        public ExpressionSequence(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public IObject Evaluate(Environment environoment)
        {
            var seq = this;
            while (true)
            {
                seq.Left.Evaluate(environoment);
                if (Right is ExpressionSequence seqRight)
                {
                    seq = seqRight;
                }
                else
                {
                    return Right.Evaluate(environoment);
                }
            }
        }

        public override string ToString()
        {
            return $"{Left}; {Right}";
        }
    }
}
