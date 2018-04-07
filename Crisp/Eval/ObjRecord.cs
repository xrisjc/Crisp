using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecord : IObj
    {
        List<string> members;
        Dictionary<string, IFn> memberFunctions;

        public ObjRecord(List<string> members, Dictionary<string, IFn> functions)
        {
            this.members = members;
            this.memberFunctions = functions;
        }

        public IFn GetMemberFunction(string name)
        {
            if (memberFunctions.TryGetValue(name, out var fn))
            {
                return fn;
            }
            else
            {
                throw new RuntimeErrorException($"cannot find member function {name}");
            }
        }

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
