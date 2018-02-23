using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.Eval
{
    class ObjStr : IObj, IIndexable, IEquatable<ObjStr>
    {
        public string Value { get; }

        public ObjStr(string value)
        {
            Value = value;
        }

        public string Print()
        {
            return $"'{Value}'";
        }

        public IObj Get(IObj index)
        {
            throw new NotImplementedException();
        }

        public IObj Set(IObj index, IObj value)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjStr);
        }

        public bool Equals(ObjStr other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
    }
}
