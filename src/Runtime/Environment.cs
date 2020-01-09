using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Environment
    {
        Environment? outer;
        Dictionary<string, Obj> values = new Dictionary<string, Obj>();

        public Environment(Environment? outer = null)
        {
            this.outer = outer;
        }

        public Obj? Get(string name)
        {
            for (Environment? e = this; e != null; e = e.outer)
                if (e.values.TryGetValue(name, out var value))
                    return value;
            return null;           
        }

        public bool Set(string name, Obj value)
        {
            for (Environment? e = this; e != null; e = e.outer)
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return true;
                }
            return false;
        }

        public bool Create(string name, Obj value) =>
            values.TryAdd(name, value);
   }
}