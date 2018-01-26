﻿namespace Crisp
{
    interface IOperatorBinary
    {
        IObj Evaluate(IObj left, IObj right);
    }

    abstract class OperatorBinaryNumber : IOperatorBinary
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
                    "arithmetic operator can only be applied to number " +
                    "values");
            }
        }

    }

    class OperatorAdd : OperatorBinaryNumber
    {
        OperatorAdd() { }

        public override double Evaluate(double left, double right)
        {
            return left + right;
        }

        public static OperatorAdd Instance { get; } = new OperatorAdd();
    }

    class OperatorSubtract : OperatorBinaryNumber
    {
        OperatorSubtract() { }

        public override double Evaluate(double left, double right)
        {
            return left - right;
        }

        public static OperatorSubtract Instance { get; } =
            new OperatorSubtract();
    }

    class OperatorMultiply : OperatorBinaryNumber
    {
        OperatorMultiply() { }

        public override double Evaluate(double left, double right)
        {
            return left * right;
        }

        public static OperatorMultiply Instance { get; } =
            new OperatorMultiply();
    }

    class OperatorDivide : OperatorBinaryNumber
    {
        OperatorDivide() { }

        public override double Evaluate(double left, double right)
        {
            return left / right;
        }

        public static OperatorDivide Instance { get; } = new OperatorDivide();
    }

    class OperatorModulo : OperatorBinaryNumber
    {
        OperatorModulo() { }

        public override double Evaluate(double left, double right)
        {
            return left % right;
        }

        public static OperatorModulo Instance { get; } = new OperatorModulo();
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
