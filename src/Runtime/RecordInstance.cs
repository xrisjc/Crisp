using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class RecordInstance
    {
        public string RecordName { get; }

        Dictionary<string, object> variables;

        public RecordInstance(string recordName, Dictionary<string, object> variables)
        {
            RecordName = recordName;
            this.variables = variables;
        }

        public object? GetAttribute(string name)
        {
            variables.TryGetValue(name, out var value);
            return value;
        }

        public bool SetAttribute(string name, object value)
        {
            if (variables.ContainsKey(name))
            {
                variables[name] = value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
