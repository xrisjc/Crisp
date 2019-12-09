using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Environment
    {
        Environment? outer;
        Dictionary<string, IObject> values = new Dictionary<string, IObject>();

        public Environment(Environment? outer = null)
        {
            this.outer = outer;
        }

        public IObject? Get(Identifier identifier)
        {
            for (Environment? e = this; e != null; e = e.outer)
                if (e.values.TryGetValue(identifier.Name, out var value))
                    return value;
            return null;           
        }

        public bool Set(string name, IObject value)
        {
            for (Environment? e = this; e != null; e = e.outer)
            {
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return true;
                }
            }
            return false;
        }

        public bool Create(string name, IObject value)
        {
            if (values.ContainsKey(name))
            {
                return false;
            }
            else
            {
                values.Add(name, value);
                return true;
            }
        }

        public void Write()
        {
            for (Environment? e = this; e != null; e = e.outer)
            {
                System.Console.WriteLine("#");
                foreach (var item in e.values)
                {
                    System.Console.WriteLine($"<{item.Key}> = {item.Value}");
                }
            }
        }
    }
}
