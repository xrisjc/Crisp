using System.Collections.Generic;

namespace Crisp.Runtime
{
    interface IEnvironment
    {
        object? Get(string name);
        bool Set(string name, object value);
    }

    class EnvironmentEmpty : IEnvironment
    {
        public object? Get(string name) => null;
        public bool Set(string name, object value) => false;
    }

    class EnvironmentExtended : IEnvironment
    {
        public string Name { get; }
        public object Value { get; set; }
        public IEnvironment Outer { get; }
        public EnvironmentExtended(string name, object value, IEnvironment outer)
        {
            Name = name;
            Value = value;
            Outer = outer;
        }
        public object? Get(string name)
        {
            if (Name == name)
                return Value;
            else
                return Outer.Get(name);
        }
        public bool Set(string name, object value)
        {
            if (Name == name)
            {
                Value = value;
                return true;
            }
            else
                return Outer.Set(name, value);
        }
    }
}
