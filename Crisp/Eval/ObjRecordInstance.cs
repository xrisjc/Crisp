using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecordInstance
    {
        ObjRecord record;
        Dictionary<string, dynamic> variables;

        public ObjRecordInstance(ObjRecord record, Dictionary<string, dynamic> variables)
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
                value = null;
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

        public ObjFn GetMemberFunction(string name)
        {
            return record.GetFunction(name);
        }
    }
}
