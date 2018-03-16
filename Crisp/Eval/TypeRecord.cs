using System.Collections.Generic;

namespace Crisp.Eval
{
    class TypeRecord : IObj, IType
    {
        List<string> members;

        public TypeRecord(List<string> members)
        {
            this.members = members;
        }

        public TypeRecord()
            : this(new List<string>())
        {
        }

        public IType Type => TypeType.Instance;

        public ObjRecordInstance Construct(Dictionary<string, IObj> initalizers)
        {
            var instanceMembers = new Dictionary<string, IObj>();
            foreach (var member in members)
            {
                instanceMembers[member] = initalizers.GetValue(member, ObjNull.Instance);
            }

            return new ObjRecordInstance(this, instanceMembers);
        }
    }
}
