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
            {
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return true;
                }
            }
            return false;
        }

        public bool Create(string name, object value)
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
