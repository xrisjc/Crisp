using Crisp.Ast;
using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    interface ICallable
    {
        CrispObject Invoke(Interpreter interpreter, CrispObject[] arguments);
    }

    class CrispObject
    {
        CrispObject? prototype;

        Dictionary<CrispObject, CrispObject> properties =
            new Dictionary<CrispObject, CrispObject>();

        public CrispObject(CrispObject? prototype = null)
        {
            this.prototype = prototype;
        }

        public static implicit operator CrispObject(bool value) =>
            new ObjectBool(value);
        public static implicit operator CrispObject(string value) =>
            new ObjectString(value);

        public CrispObject Get(CrispObject key)
        {
            for (CrispObject? o = this; o != null; o = o.prototype)
                if (o.properties.TryGetValue(key, out var value))
                    return value;
            return new ObjectNull();           
        }

        public void Set(CrispObject key, CrispObject value)
        {
            properties[key] = value;
        }

        public virtual ObjectBool IsTruthy() => true;
    }

    class ObjectBool : CrispObject, IEquatable<ObjectBool>
    {
        public bool Value { get; }

        public ObjectBool(bool value) { Value = value; }

        public static implicit operator ObjectBool(bool value) => new ObjectBool(value);
        public static implicit operator bool(ObjectBool value) => value.Value;

        public static ObjectBool operator !(ObjectBool a) => !a.Value;

        public override ObjectBool IsTruthy() => Value;

        public override string ToString() => Value ? "true" : "false";

        public override bool Equals(object? obj) => Equals(obj as ObjectBool);
        public bool Equals(ObjectBool? other) => other != null && Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

    class ObjectFunction : CrispObject, ICallable
    {
        public Function Definition { get; }

        public ObjectFunction(Function definition)
        {
            Definition = definition;
        }
        
        public CrispObject Invoke(
            Interpreter interpreter,
            CrispObject[] arguments)
        {
            var environment = interpreter.Environment;
            var parameters = Definition.Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                CrispObject value;
                if (i < arguments.Length)
                    value = arguments[i];
                else
                    value = new ObjectNull();
                
                if (!environment.Create(parameters[i].Name, value))
                    throw new RuntimeErrorException(
                        parameters[i].Position,
                        $"Parameter {parameters[i].Name} already bound.");
            }
            return interpreter.Evaluate(Definition.Body);
        }
    }

    class ObjectNull : CrispObject, IEquatable<ObjectNull>
    {
        public override ObjectBool IsTruthy() => false;

        public override string ToString() => "null";

        public override bool Equals(object? obj) => Equals(obj as ObjectNull);
        public bool Equals(ObjectNull? other) => other != null;
        public override int GetHashCode() => 0;
    }

    class ObjectNumber : CrispObject, IEquatable<ObjectNumber>
    {
        public double Value { get; }

        public ObjectNumber(double value) { Value = value; }

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

        public override bool Equals(object? obj) => Equals(obj as ObjectNumber);
        public bool Equals(ObjectNumber? other) => other != null && Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
    }

    class ObjectString : CrispObject, IEquatable<ObjectString>
    {
        public string Value { get; }

        public ObjectString(string value) { Value = value; }

        public static implicit operator ObjectString(string value) =>
            new ObjectString(value);

        public override string ToString() => Value;

        public override bool Equals(object? obj) => Equals(obj as ObjectString);
        public bool Equals(ObjectString? other) => other != null && Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
    }
}