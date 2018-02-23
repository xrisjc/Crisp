using System;

namespace Crisp.Eval
{
    class ObjInt : IObj, INumeric, IEquatable<ObjInt>, IComparable<IObj>
    {
        public long Value { get; }

        public ObjInt(long value)
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
            return (right is ObjInt i) ? new ObjInt(Value + i.Value) : right.AddTo(this);
        }

        public INumeric DivideBy(INumeric right)
        {
            return (right is ObjInt i) ? new ObjInt(Value / i.Value) : right.AddTo(this);
        }

        public INumeric ModuloOf(INumeric right)
        {
            return (right is ObjInt i) ? new ObjInt(Value % i.Value) : right.AddTo(this);
        }

        public INumeric MultiplyBy(INumeric right)
        {
            return (right is ObjInt i) ? new ObjInt(Value * i.Value) : right.AddTo(this);
        }

        public INumeric SubtractBy(INumeric right)
        {
            return (right is ObjInt i) ? new ObjInt(Value - i.Value) : right.AddTo(this);
        }

        public ObjFloat ToFloat()
        {
            return new ObjFloat(Value);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjInt);
        }

        public bool Equals(ObjInt other)
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
                case Obj
            }

            throw new RuntimeErrorException(
                $"Cannot compare {Print()} and {other.Print()}");

        }
    }
}
