using System.Collections.Generic;

namespace Crisp.Runtime
{
    class RecordInstance : IEntity
    {
        Record record;
        Dictionary<string, dynamic> variables;

        public RecordInstance(Record record, Dictionary<string, dynamic> variables)
        {
            this.record = record;
            this.variables = variables;
        }

        public bool GetAttribute(string name, out dynamic value)
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

        public bool SetAttribute(string name, dynamic value)
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

        public bool GetMethod(string name, out Function method)
        {
            return record.GetInstanceMethod(name, out method);
        }
    }
}
