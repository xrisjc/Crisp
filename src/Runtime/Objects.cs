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

    class ObjectFunction : CrispObject, ICallable
    {
        public Function Definition { get; }

        public ObjectFunction(CrispObject prototype, Function definition)
            : base(prototype)
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
                    value = interpreter.System.Null;
                
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
        public ObjectNull(CrispObject prototype) : base(prototype) { }
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