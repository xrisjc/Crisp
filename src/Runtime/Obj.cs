using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Obj
    {
        Obj? prototype;
        
        Dictionary<Obj, Obj> properties;

        public object? Value { get; }

        public Obj(Obj? prototype, object? value = null)
        {
            this.prototype = prototype;
            properties = new Dictionary<Obj, Obj>();
            Value = value;
        }

        public virtual Obj? LookupProperty(Obj key)
        {
            for (Obj? o = this; o != null; o = o.prototype)
                if (o.properties.TryGetValue(key, out var value))
                    return value;
            return null;
        }

        public void SetProperty(Obj key, Obj value)
        {
            properties[key] = value;
        }
        
        public bool Is(Obj obj)
        {
            for (Obj? o = this; o != null; o = o.prototype)
                if (ReferenceEquals(o, obj))
                    return true;
            return false;
        }

        public virtual Obj Beget() => new Obj(this);

        public override string? ToString()
        {
            if (Value != null) return Value.ToString();
            return base.ToString();
        }
        
        public override bool Equals(object? obj) => this.Equals(obj as Obj);
        
        public bool Equals(Obj? other)
        {
            if (other == null) return false;
            if (Value != null) return Value.Equals(other.Value);
            return ReferenceEquals(this, other);
        }
        
        public override int GetHashCode()
        {
            if (Value != null) return Value.GetHashCode();
            return base.GetHashCode();
        }
    }

    delegate Obj Callable(Interpreter interpreter, Obj? self, Obj[] arguments);

    class Null { }
}
