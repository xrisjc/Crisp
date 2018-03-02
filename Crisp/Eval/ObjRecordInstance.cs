using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecordInstance : IObj, IMemberGet
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
                return (value, GetStatus.Found);
            }
            else
            {
                return (ObjNull.Instance, GetStatus.NotFound);
            }
        }

        public string Print()
        {
            return "<RecordInstance>";
        }
    }
}
