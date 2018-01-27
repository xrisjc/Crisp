namespace Crisp
{
    interface IOperatorBinary
    {
        IObj Evaluate(IObj left, IObj right);
    }

    abstract class OperatorBinaryNumberArithmetic : IOperatorBinary
    {
        public abstract double Evaluate(double left, double right);

        public IObj Evaluate(IObj left, IObj right)
        {
            if ((left is Obj<double> numLeft) &&
                (right is Obj<double> numRight))
            {
                var result = Evaluate(numLeft.Value, numRight.Value);
                return Obj.Create(result);
            }
            else
            {
                throw new RuntimeErrorException(
                    "arithmetic operators can only be applied to number " +
                    "values");
            }
        }
    }

    abstract class OperatorBinaryNumberRelational : IOperatorBinary
    {
        public abstract bool Evaluate(double left, double right);

        public IObj Evaluate(IObj left, IObj right)
        {
            if ((left is Obj<double> numLeft) &&
                (right is Obj<double> numRight))
            {
                var result = Evaluate(numLeft.Value, numRight.Value);
                return result ? Obj.True : Obj.False;
            }
            else
            {
                throw new RuntimeErrorException(
                    "relational operators can only be applied to number " +
                    "values");
            }
        }
    }

    class OperatorAdd : OperatorBinaryNumberArithmetic
    {
        OperatorAdd() { }

        public override double Evaluate(double left, double right)
        {
            return left + right;
        }

        public static OperatorAdd Instance { get; } = new OperatorAdd();
    }

    class OperatorSubtract : OperatorBinaryNumberArithmetic
    {
        OperatorSubtract() { }

        public override double Evaluate(double left, double right)
        {
            return left - right;
        }

        public static OperatorSubtract Instance { get; } =
            new OperatorSubtract();
    }

    class OperatorMultiply : OperatorBinaryNumberArithmetic
    {
        OperatorMultiply() { }

        public override double Evaluate(double left, double right)
        {
            return left * right;
        }

        public static OperatorMultiply Instance { get; } =
            new OperatorMultiply();
    }

    class OperatorDivide : OperatorBinaryNumberArithmetic
    {
        OperatorDivide() { }

        public override double Evaluate(double left, double right)
        {
            return left / right;
        }

        public static OperatorDivide Instance { get; } = new OperatorDivide();
    }

    class OperatorModulo : OperatorBinaryNumberArithmetic
    {
        OperatorModulo() { }

        public override double Evaluate(double left, double right)
        {
            return left % right;
        }

        public static OperatorModulo Instance { get; } = new OperatorModulo();
    }

    class OperatorLessThan : OperatorBinaryNumberRelational
    {
        OperatorLessThan() { }

        public override bool Evaluate(double left, double right)
        {
            return left < right;
        }

        public static OperatorLessThan Instance { get; } =
            new OperatorLessThan();
    }

    class OperatorLessThanOrEqualTo : OperatorBinaryNumberRelational
    {
        OperatorLessThanOrEqualTo() { }

        public override bool Evaluate(double left, double right)
        {
            return left <= right;
        }

        public static OperatorLessThanOrEqualTo Instance { get; } =
            new OperatorLessThanOrEqualTo();
    }

    class OperatorGreaterThan : OperatorBinaryNumberRelational
    {
        OperatorGreaterThan() { }

        public override bool Evaluate(double left, double right)
        {
            return left > right;
        }

        public static OperatorGreaterThan Instance { get; } =
            new OperatorGreaterThan();
    }

    class OperatorGreaterThanOrEqualTo : OperatorBinaryNumberRelational
    {
        OperatorGreaterThanOrEqualTo() { }

        public override bool Evaluate(double left, double right)
        {
            return left >= right;
        }

        public static OperatorGreaterThanOrEqualTo Instance { get; } =
            new OperatorGreaterThanOrEqualTo();
    }

    class OperatorEquals : IOperatorBinary
    {
        OperatorEquals() { }

        public IObj Evaluate(IObj left, IObj right)
        {
            return left.Equals(right) ? Obj.True : Obj.False;
        }

        public static OperatorEquals Instance { get; } = new OperatorEquals();
    }

    class OperatorInequalTo : IOperatorBinary
    {
        OperatorInequalTo() { }

        public IObj Evaluate(IObj left, IObj right)
        {
            return left.Equals(right) ? Obj.False : Obj.True;
        }

        public static OperatorInequalTo Instance { get; } =
            new OperatorInequalTo();
    }

}
