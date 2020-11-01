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
        readonly string name;
        object value;
        readonly IEnvironment outer;
        public EnvironmentExtended(string name, object value, IEnvironment outer)
        {
            this.name = name;
            this.value = value;
            this.outer = outer;
        }
        public object? Get(string name)
        {
            if (this.name == name)
                return value;
            else
                return outer.Get(name);
        }
        public bool Set(string name, object value)
        {
            if (this.name == name)
            {
                this.value = value;
                return true;
            }
            else
                return outer.Set(name, value);
        }
    }
}
