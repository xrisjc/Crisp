using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Environment
    {
        Environment? outer;
        Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment(Environment? outer = null)
        {
            this.outer = outer;
        }

        public object? Get(string name)
        {
            for (Environment? e = this; e != null; e = e.outer)
                if (e.values.TryGetValue(name, out var value))
                    return value;
            return null;           
        }

        public bool Set(string name, object value)
        {
            for (Environment? e = this; e != null; e = e.outer)
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return true;
                }
            return false;
        }

        public bool Create(string name, object value) =>
            values.TryAdd(name, value);
   }
}