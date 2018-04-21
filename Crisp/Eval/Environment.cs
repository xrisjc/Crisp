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

        public bool Get(string name, out dynamic value)
        {
            for (var e = this; e != null; e = e.outer)
            {
                if (e.values.TryGetValue(name, out value))
                {
                    return true;
                }
            }

            value = Null.Instance;
            return false;
        }

        public bool Set(string name, dynamic value)
        {
            for (var e = this; e != null; e = e.outer)
            {
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return true;
                }
            }
            return false;
        }

        public bool Create(string name, dynamic value)
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
    }
}
