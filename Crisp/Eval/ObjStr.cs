using System;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjStr : IObj, IIndexGet, IEquatable<ObjStr>
    {
        public string Value { get; }

        public ObjStr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public IType Type => TypeString.Instance;

        public IObj IndexGet(IObj index)
        {
            switch (index)
            {
                case ObjInt i
                when 0 <= i.Value && i.Value < Value.Length:
                    return new ObjStr(Value[i.Value].ToString());

                case ObjInt i:
                    throw new RuntimeErrorException("Index out of bounds.");

                default:
                    throw new RuntimeErrorException("Can only index a string with an integer.");
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjStr);
        }

        public bool Equals(ObjStr other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
    }
}
