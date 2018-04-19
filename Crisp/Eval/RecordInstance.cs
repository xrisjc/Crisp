using System.Collections.Generic;

namespace Crisp.Eval
{
    class RecordInstance
    {
        Record record;
        Dictionary<string, dynamic> variables;

        public RecordInstance(Record record, Dictionary<string, dynamic> variables)
        {
            this.record = record;
            this.variables = variables;
        }

        public bool MemberGet(string name, out dynamic value)
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

        public bool MemberSet(string name, dynamic value)
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

        public Function GetMemberFunction(string name)
        {
            return record.GetFunction(name);
        }
    }
}
