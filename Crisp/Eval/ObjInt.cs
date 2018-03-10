using System;

namespace Crisp.Eval
{
    class ObjInt : IObj, IEquatable<ObjInt>
    {
        public int Value { get; }

        public ObjInt(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
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
    }
}
