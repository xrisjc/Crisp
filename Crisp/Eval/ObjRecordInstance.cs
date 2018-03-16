using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecordInstance : IObj, IMemberGet, IMemberSet
    {
        TypeRecord record;
        Dictionary<string, IObj> members;

        public ObjRecordInstance(TypeRecord record, Dictionary<string, IObj> members)
        {
            this.record = record;
            this.members = members;
        }

        public IType Type => record;

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
    }
}
