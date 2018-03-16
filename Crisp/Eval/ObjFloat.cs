using System;

namespace Crisp.Eval
{
    class ObjFloat : IObj, IEquatable<ObjFloat>
    {
        public double Value { get; }

        public ObjFloat(double value)
        {
            Value = value;
        }

        public IType Type => TypeFloat.Instance;

        public override string ToString()
        {
            return Value.ToString();
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
    }
}
