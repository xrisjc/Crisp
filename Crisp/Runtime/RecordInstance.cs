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

        public bool GetAttribute(string name, out object value)
        {
            if (variables.TryGetValue(name, out value))
            {
                return true;
            }
            else
            {
                value = Null.Instance;
                return false;
            }
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
