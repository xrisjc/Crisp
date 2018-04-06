using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecordInstance : IObj, IMemberGet, IMemberSet
    {
        ObjRecord record;
        Dictionary<string, IObj> members;

        public ObjRecordInstance(ObjRecord record, Dictionary<string, IObj> members)
        {
            this.record = record;
            this.members = members;
        }

        public (IObj, MemberStatus) MemberGet(string name)
        {
            if (members.TryGetValue(name, out var value))
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
            if (members.ContainsKey(name))
            {
                members[name] = value;
                return (value, MemberStatus.Ok);
            }
            else
            {
                return (ObjNull.Instance, MemberStatus.NotFound);
            }
        }

        public IFn GetMemberFunction(string name)
        {
            return record.GetMemberFunction(name);
        }
    }
}
