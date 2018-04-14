using System.Collections.Generic;

namespace Crisp.Eval
{
    class Environment
    {
        Environment outer;
        Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();

        public Environment(Environment outer = null)
        {
            this.outer = outer;
        }

        public dynamic Get(string name)
        {
            for (var e = this; e != null; e = e.outer)
            {
                if (e.values.TryGetValue(name, out var value))
                {
                    return value;
                }
            }

            throw new RuntimeErrorException($"reference to undeclared variable {name}");
        }

        public dynamic Set(string name, dynamic value)
        {
            for (var e = this; e != null; e = e.outer)
            {
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return value;
                }
            }

            throw new RuntimeErrorException($"reference to undeclared variable {name}");
        }

        public dynamic Create(string name, dynamic value)
        {
            if (values.ContainsKey(name))
            {
                throw new RuntimeErrorException($"a variable named '{name}' already exists");
            }
            else
            {
                values.Add(name, value);
                return value;
            }
        }
    }
}
