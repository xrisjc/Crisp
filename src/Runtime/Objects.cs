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
        public CrispObject? Prototype { get; }
        public Dictionary<CrispObject, CrispObject> Properties { get; }
        public CrispObject(CrispObject? prototype)
        {
            Prototype = prototype;
            Properties = new Dictionary<CrispObject, CrispObject>();
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

        public override bool Equals(object? obj)
            => Equals(obj as ObjectString);
        public bool Equals(ObjectString? other)
            => other != null && Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
    }
}