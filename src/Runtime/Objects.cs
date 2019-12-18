using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class CrispObject
    {
        public CrispObject? Prototype { get; }
        public Dictionary<CrispObject, CrispObject> Properties { get; }
        public CrispObject(CrispObject? prototype)
        {
            Prototype = prototype;
            Properties = new Dictionary<CrispObject, CrispObject>();
        }
        public override string ToString() => "<obj>";
    }

    class ObjectBool : CrispObject, IEquatable<ObjectBool>
    {
        public bool Value { get; }

        public ObjectBool(CrispObject? prototype, bool value)
            : base(null)
        {
            Value = value;
        }
        public override string ToString() => Value ? "true" : "false";
        public override bool Equals(object? obj) => Equals(obj as ObjectBool);
        public bool Equals(ObjectBool? other) => other != null && Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

    class ObjectFunction : CrispObject
    {
        public int Offset { get; }

        public ObjectFunction(CrispObject prototype, int offset)
            : base(prototype)
        {
            Offset = offset;
        }
        public override string ToString() => string.Format("<fn@{0:D8}>", Offset);
    }

    class ObjectNull : CrispObject, IEquatable<ObjectNull>
    {
        public ObjectNull(CrispObject prototype) : base(prototype) { }
        public override bool Equals(object? obj) => Equals(obj as ObjectNull);
        public bool Equals(ObjectNull? other) => other != null;
        public override int GetHashCode() => 0;
    }

    class ObjectNumber : CrispObject, IEquatable<ObjectNumber>
    {
        public double Value { get; }

        public ObjectNumber(CrispObject prototype, double value)
            : base(prototype)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
        public override bool Equals(object? obj) => Equals(obj as ObjectNumber);
        public bool Equals(ObjectNumber? other) => other != null && Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
    }

    class ObjectString : CrispObject, IEquatable<ObjectString>
    {
        public string Value { get; }
        public ObjectString(CrispObject prototype, string value)
            : base(prototype)
        {
            Value = value;
        }

        public override string ToString() => Value;
        public override bool Equals(object? obj)
            => Equals(obj as ObjectString);
        public bool Equals(ObjectString? other)
            => other != null && Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
    }
}