using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    abstract class CrispObject
    {
        CrispObject? prototype;

        Dictionary<CrispObject, CrispObject> properties =
            new Dictionary<CrispObject, CrispObject>();

        public CrispObject(CrispObject? prototype = null)
        {
            this.prototype = prototype;
        }

        public CrispObject? Get(CrispObject key)
        {
            for (CrispObject? o = this; o != null; o = o.prototype)
                if (o.properties.TryGetValue(key, out var value))
                    return value;
            return null;           
        }

        public void Set(CrispObject key, CrispObject value)
        {
            // If the key exists somewhere, set it.
            for (CrispObject? o = this; o != null; o = o.prototype)
                if (o.properties.ContainsKey(key))
                {
                    o.properties[key] = value;
                    return;
                }

            // Key doesn't exist so set it in this object.
            properties[key] = value;
        }

        public virtual ObjectBool IsTruthy() => true;
        
        public abstract ObjectBool Eq(CrispObject other);
    }

    class ObjectBool : CrispObject
    {
        public bool Value { get; }

        public ObjectBool(bool value) { Value = value; }

        public static implicit operator ObjectBool(bool value) => new ObjectBool(value);

        public static implicit operator bool(ObjectBool value) => value.Value;

        public static ObjectBool operator !(ObjectBool a) => !a.Value;

        public override ObjectBool IsTruthy() => Value;

        public override ObjectBool Eq(CrispObject other) => other is ObjectBool x && Value == x.Value;

        public override string ToString() => Value ? "true" : "false";
    }

    class ObjectFunction : CrispObject
    {
        public Function Value { get; }

        public ObjectFunction(Function function) { Value = function; }

        public override ObjectBool Eq(CrispObject other) => ReferenceEquals(this, other);
    }

    class ObjectNull : CrispObject
    {
        public override ObjectBool IsTruthy() => false;

        public override ObjectBool Eq(CrispObject other) => other is ObjectNull;

        public override string ToString() => "null";
    }

    class ObjectNumber : CrispObject
    {
        public double Value { get; }

        public ObjectNumber(double value) { Value = value; }
        
        public override ObjectBool Eq(CrispObject other) => other is ObjectNumber n && Value == n.Value;
        
        public static implicit operator ObjectNumber(double value) => new ObjectNumber(value);
        public static ObjectNumber operator +(ObjectNumber a, ObjectNumber b) => a.Value + b.Value;
        public static ObjectNumber operator -(ObjectNumber a, ObjectNumber b) => a.Value - b.Value;
        public static ObjectNumber operator *(ObjectNumber a, ObjectNumber b) => a.Value * b.Value;
        public static ObjectNumber operator /(ObjectNumber a, ObjectNumber b) => a.Value / b.Value;
        public static ObjectNumber operator %(ObjectNumber a, ObjectNumber b) => a.Value % b.Value;
        public static ObjectBool operator <(ObjectNumber a, ObjectNumber b) => a.Value < b.Value;
        public static ObjectBool operator <=(ObjectNumber a, ObjectNumber b) => a.Value <= b.Value;
        public static ObjectBool operator >(ObjectNumber a, ObjectNumber b) => a.Value > b.Value;
        public static ObjectBool operator >=(ObjectNumber a, ObjectNumber b) => a.Value >= b.Value;
        public static ObjectNumber operator -(ObjectNumber a) => -a.Value;

        public override string ToString() => Value.ToString();
    }
    
    class ObjectObject : CrispObject
    {
        public override ObjectBool Eq(CrispObject other) => ReferenceEquals(this, other);

        public ObjectObject(CrispObject? prototype = null) : base(prototype) { }
    }

    class ObjectString : CrispObject
    {
        public string Value { get; }

        public ObjectString(string value) { Value = value; }
        
        public override ObjectBool Eq(CrispObject other) => other is ObjectString s && Value == s.Value;
        
        public static implicit operator ObjectString(string value) => new ObjectString(value);

        public override string ToString() => Value;
    }
}