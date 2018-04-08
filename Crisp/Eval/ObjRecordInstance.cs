using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecordInstance : IObj, IMemberGet, IMemberSet
    {
        ObjRecord record;
        Dictionary<string, IObj> variables;

        public ObjRecordInstance(ObjRecord record, Dictionary<string, IObj> variables)
        {
            this.record = record;
            this.variables = variables;
        }

        public (IObj, MemberStatus) MemberGet(string name)
        {
            if (variables.TryGetValue(name, out var value))
            {
                return (value, MemberStatus.Ok);
            }
            else
            {
                return (ObjNull.Instance, MemberStatus.NotFound);
            }
        }

        public (IObj, MemberStatus) MemberSet(string name, IObj value)
        {
            if (variables.ContainsKey(name))
            {
                variables[name] = value;
                return (value, MemberStatus.Ok);
            }
            else
            {
                return (ObjNull.Instance, MemberStatus.NotFound);
            }
        }

        public IFn GetMemberFunction(string name)
        {
            return record.GetFunction(name);
        }
    }
}
