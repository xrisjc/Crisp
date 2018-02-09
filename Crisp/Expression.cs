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

    class ExpressionLogicalAnd : IExpression
    {
        IExpression left, right;

        public ExpressionLogicalAnd(IExpression left, IExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public IObj Evaluate(Environment environment)
        {
            var objLeft = left.Evaluate(environment);
            if (objLeft is Obj<bool> boolLeft)
            {
                if (boolLeft.Value == false)
                {
                    return boolLeft;
                }

                var objRight = right.Evaluate(environment);
                if (objRight is Obj<bool> boolRight)
                {
                    return boolRight;
                }
                else
                {
                    throw new RuntimeErrorException(
                        "Right hand side of 'and' must be Boolean.");
                }
            }
            else
            {
                throw new RuntimeErrorException(
                    "Left hand side of 'and' must be Boolean.");
            }
        }
    }

    class ExpressionLogicalOr : IExpression
    {
        IExpression left, right;

        public ExpressionLogicalOr(IExpression left, IExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public IObj Evaluate(Environment environment)
        {
            var objLeft = left.Evaluate(environment);
            if (objLeft is Obj<bool> boolLeft)
            {
                if (boolLeft.Value)
                {
                    return boolLeft;
                }

                var objRight = right.Evaluate(environment);
                if (objRight is Obj<bool> boolRight)
                {
                    return boolRight;
                }
                else
                {
                    throw new RuntimeErrorException(
                        "Right hand side of 'and' must be Boolean.");
                }
            }
            else
            {
                throw new RuntimeErrorException(
                    "Left hand side of 'or' must be Boolean.");
            }
        }
    }
}
