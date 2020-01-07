using Crisp.Ast;
using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class CrispObject
    {
        CrispObject? prototype;
        Dictionary<CrispObject, CrispObject> properties;
        public CrispObject(CrispObject? prototype)
        {
            this.prototype = prototype;
            properties = new Dictionary<CrispObject, CrispObject>();
        }
        public virtual CrispObject? LookupProperty(CrispObject key)
        {
            for (CrispObject? o = this; o != null; o = o.prototype)
                if (o.properties.TryGetValue(key, out var value))
                    return value;
            return null;
        }
        public virtual void SetProperty(CrispObject key, CrispObject value)
        {
            properties[key] = value;
        }
        public bool Is(CrispObject obj)
        {
            for (CrispObject? o = this; o != null; o = o.prototype)
                if (ReferenceEquals(o, obj))
                    return true;
            return false;
        }
        public virtual CrispObject Beget() => new CrispObject(this);
        public virtual bool IsTruthy() => true;
    }

    class ObjectList : CrispObject
    {
        public List<CrispObject> Items { get; }
        public ObjectList(CrispObject? prototype, List<CrispObject> items)
            : base(prototype)
        {
            Items = items;
        }

        public CrispObject Add(IEnumerable<CrispObject> items)
        {
            Items.AddRange(items);
            return this;
        }

        public override CrispObject? LookupProperty(CrispObject key)
        {
            if (AsIndex(key) is int index)
            {
                return Items[index];
            }
            
            return base.LookupProperty(key);
        }
        public override void SetProperty(CrispObject key, CrispObject value)
        {
            if (AsIndex(key) is int index)
                Items[index] = value;
            else
                base.SetProperty(key, value);
        }
        
        int? AsIndex(CrispObject obj)
        {
            if (obj is ObjectNumber num)
            {
                var floor = Math.Floor(num.Value);
                if (floor == Math.Ceiling(num.Value))
                {
                    var index = (int)floor;
                    // TODO: Have a runtime error if we're out of bounds.
                    if (index >= 0 && index < Items.Count)
                        return index;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return string.Join(", ", Items);
        }
    }

    class ObjectBool : CrispObject, IEquatable<ObjectBool>
    {
        public bool Value { get; }

        public ObjectBool(CrispObject? prototype, bool value)
            : base(prototype)
        {
            Value = value;
        }
        public override bool IsTruthy() => Value;
        public override string ToString() => Value ? "true" : "false";
        public override bool Equals(object? obj) => Equals(obj as ObjectBool);
        public bool Equals(ObjectBool? other) => other != null && Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

    delegate CrispObject Callable(Interpreter interpreter, CrispObject? self, CrispObject[] arguments);

    class ObjectCallable : CrispObject
    {
        Callable callable;
        public ObjectCallable(CrispObject prototype, Callable callable)
            : base(prototype)
        {
            this.callable = callable;
        }
        public CrispObject Call(Interpreter interpreter, CrispObject? self, CrispObject[] arguments)
            => callable(interpreter, self, arguments);
    }

    class ObjectNull : CrispObject, IEquatable<ObjectNull>
    {
        public ObjectNull(CrispObject prototype) : base(prototype) { }
        public override bool IsTruthy() => false;
        public override string ToString() => "null";
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