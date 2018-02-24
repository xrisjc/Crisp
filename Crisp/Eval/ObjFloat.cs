using System;

namespace Crisp.Eval
{
    class ObjFloat : IObj, INumeric, IEquatable<ObjFloat>, IComparable<IObj>
    {
        public double Value { get; }

        public ObjFloat(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string Print()
        {
            return ToString();
        }

        public INumeric AddTo(INumeric right)
        {
            return new ObjFloat(Value + right.ToFloat().Value);
        }

        public INumeric DivideBy(INumeric right)
        {
            return new ObjFloat(Value / right.ToFloat().Value);
        }

        public INumeric ModuloOf(INumeric right)
        {
            return new ObjFloat(Value % right.ToFloat().Value);
        }

        public INumeric MultiplyBy(INumeric right)
        {
            return new ObjFloat(Value * right.ToFloat().Value);
        }

        public INumeric SubtractBy(INumeric right)
        {
            return new ObjFloat(Value - right.ToFloat().Value);
        }

        public ObjFloat ToFloat()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjFloat);
        }

        public bool Equals(ObjFloat other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        public int CompareTo(IObj other)
        {
            switch (other)
            {
                case ObjInt iOther:
                    return Value.CompareTo(iOther.Value);

                case ObjFloat fOther:
                    return Value.CompareTo(fOther.Value);
            }

            throw new RuntimeErrorException(
                $"Cannot compare {Print()} and {other.Print()}");
        }
    }
}