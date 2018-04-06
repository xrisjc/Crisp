using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecord : IObj
    {
        private List<string> members;
        private Dictionary<string, IFn> memberFunctions = new Dictionary<string, IFn>();

        public ObjRecord(List<string> members)
        {
            this.members = members;
        }

        public void AddMemberFunction(string name, IFn function)
        {
            if (memberFunctions.ContainsKey(name))
            {
                throw new RuntimeErrorException(
                    $"record already contains member function \"<{name}>\"");
            }
            else
            {
                memberFunctions.Add(name, function);
            }
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
