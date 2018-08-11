using System.Collections.Generic;

namespace Crisp.Runtime
{
    class RecordInstance : Entity
    {
        Record record;
        Dictionary<string, object> variables;

        public RecordInstance(Record record, Dictionary<string, object> variables)
        {
            this.record = record;
            this.variables = variables;
        }

        public override bool GetAttribute(string name, out object value)
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

        public override bool SetAttribute(string name, object value)
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

        public override bool SendMessage(string name, Evaluator evaluator)
        {
            if (record.Functions.TryGetValue(name, out var method))
            {
                evaluator.Invoke(method);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
