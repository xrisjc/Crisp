using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Eval
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

        public IObj Get(Identifier identifier)
        {
            return Get(identifier.Name);
        }

        public IObj Set(string name, IObj value)
        {
            for (Environment e = this; e != null; e = e.outer)
            {
                if (e.values.ContainsKey(name))
                {
                    e.values[name] = value;
                    return value;
                }
            }

            throw new RuntimeErrorException($"reference to undeclared variable {name}");
        }

        public IObj Set(Identifier identifier, IObj value)
        {
            return Set(identifier.Name, value);
        }

        public IObj Create(string name, IObj value)
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

        public IObj Create(Identifier identifier, IObj value)
        {
            return Create(identifier.Name, value);
        }
    }
}
