using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Environment
    {
        Environment? outer;
        Dictionary<string, CrispObject> values =
            new Dictionary<string, CrispObject>();

        public Environment(Environment? outer = null)
        {
            this.outer = outer;
        }

        public CrispObject? Get(string name)
        {
            for (Environment? e = this; e != null; e = e.outer)
                if (e.values.TryGetValue(name, out var value))
                    return value;
            return null;           
        }

        public bool Set(string name, CrispObject value)
        {
            for (Environment? e = this; e != null; e = e.outer)
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return true;
                }
            return false;
        }

        public bool Create(string name, CrispObject value) =>
            values.TryAdd(name, value);
   }
}