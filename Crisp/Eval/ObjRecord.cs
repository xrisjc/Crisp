﻿using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecord : IObj
    {
        List<string> members;

        public ObjRecord(List<string> members)
        {
            this.members = members;
        }

        public ObjRecord()
            : this(new List<string>())
        {
        }

        public override string ToString()
        {
            return "<record>";
        }

        public string Print()
        {
            return ToString();
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
