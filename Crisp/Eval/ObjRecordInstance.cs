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

        public (IObj, GetStatus) MemberGet(string name)
        {
            if (members.TryGetValue(name, out var value))
            {
                return (value, GetStatus.Got);
            }
            else
            {
                return (ObjNull.Instance, GetStatus.NotFound);
            }
        }

        public (IObj, SetStatus) MemberSet(string name, IObj value)
        {
            if (members.ContainsKey(name))
            {
                members[name] = value;
                return (value, SetStatus.Set);
            }
            else
            {
                return (ObjNull.Instance, SetStatus.NotFound);
            }
        }
    }
}
