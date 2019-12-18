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

    class Environment2
    {
        public Environment2? Outer { get; }
        Dictionary<CrispObject, CrispObject> values =
            new Dictionary<CrispObject, CrispObject>();

        public Environment2(Environment2? outer = null)
        {
            Outer = outer;
        }

        public CrispObject? Get(CrispObject key)
        {
            for (Environment2? e = this; e != null; e = e.Outer)
                if (e.values.TryGetValue(key, out var value))
                    return value;
            return null;
        }

        public void Set(CrispObject key, CrispObject value)
        {
            for (Environment2? e = this; e != null; e = e.Outer)
                if (e.values.ContainsKey(key))
                {
                    e.values[key] = value;
                    break;
                }
        }

        public void Create(CrispObject key, CrispObject value)
            => values.Add(key, value);
    }
}