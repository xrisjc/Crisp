using System.Collections.Generic;

namespace Crisp.Runtime
{
    class RecordInstance : Entity
    {
        Record record;
        Dictionary<string, dynamic> variables;

        public RecordInstance(Record record, Dictionary<string, dynamic> variables)
        {
            this.record = record;
            this.variables = variables;
        }

        public override bool GetAttribute(string name, out dynamic value)
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

        public override bool SetAttribute(string name, dynamic value)
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
            if (record.GetInstanceMethod(name, out var method))
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
