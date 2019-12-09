using Crisp.Ast;

namespace Crisp.Runtime
{
    interface IObject
    {
        ObjectBool IsTruthy() => true;
        
        ObjectBool Eq(IObject other);
    }

    abstract class ValueWrapper<T>
    {
        public T Value { get; }

        public ValueWrapper(T value) { Value = value; }   
    }

    class ObjectBool : ValueWrapper<bool>, IObject
    {
        public ObjectBool(bool value) : base(value) { }

        public static implicit operator ObjectBool(bool value) => new ObjectBool(value);

        public static implicit operator bool(ObjectBool value) => value.Value;

        public static ObjectBool operator !(ObjectBool a) => !a.Value;

        public ObjectBool IsTruthy() => Value;

        public ObjectBool Eq(IObject other) => other is ObjectBool x && Value == x.Value;

        public override string ToString() => Value ? "true" : "false";
    }

    class ObjectFunction : ValueWrapper<Function>, IObject
    {
        public ObjectFunction(Function function) : base(function) { }

        public ObjectBool Eq(IObject other) => ReferenceEquals(this, other);
    }

    class ObjectNull : IObject
    {
        public ObjectBool IsTruthy() => false;

        public ObjectBool Eq(IObject other) => other is ObjectNull;

        public override string ToString() => "null";
    }

    class ObjectNumber : ValueWrapper<double>, IObject
    {
        public ObjectNumber(double value) : base(value) { }
        
        public ObjectBool Eq(IObject other) => other is ObjectNumber n && Value == n.Value;
        
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
    
    class ObjectString : ValueWrapper<string>, IObject
    {
        public ObjectString(string value) : base(value) { }
        
        public ObjectBool Eq(IObject other) => other is ObjectString s && Value == s.Value;
        
        public static implicit operator ObjectString(string value) => new ObjectString(value);
        public static ObjectString operator +(ObjectString a, ObjectString b) => a.Value + b.Value;

        public override string ToString() => Value;
    }
}