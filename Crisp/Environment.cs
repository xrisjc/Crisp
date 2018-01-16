﻿using System.Collections.Generic;

namespace Crisp
{
    class Environment
    {
        readonly Environment outer;
        protected readonly Dictionary<string, IObj> values = new Dictionary<string, IObj>();

        public Environment(Environment outer = null)
        {
            this.outer = outer;
        }

        public IObj Get(string name)
        {
            for (Environment e = this; e != null; e = e.outer)
            {
                if (e.values.TryGetValue(name, out IObj value))
                {
                    return value;
                }
            }

            throw new RuntimeErrorException($"reference to undeclared variable {name}");
        }

        public void Set(string name, IObj value)
        {
            for (Environment e = this; e != null; e = e.outer)
            {
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return;
                }
            }

            throw new RuntimeErrorException($"reference to undeclared variable {name}");
        }

        public void Create(string name, IObj value)
        {
            if (values.ContainsKey(name))
            {
                throw new RuntimeErrorException($"a variable named '{name}' already exists");
            }
            else
            {
                values.Add(name, value);
            }
        }
    }
}
