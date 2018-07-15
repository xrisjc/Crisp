using System.Collections.Generic;

namespace Crisp.Runtime
{
    abstract class Entity
    {
        public virtual bool GetAttribute(string name, out object value)
        {
            value = Null.Instance;
            return false;
        }

        public virtual bool SetAttribute(string name, object value)
        {
            value = Null.Instance;
            return false;
        }

        public virtual bool SendMessage(string name, List<object> arguments, out object value)
        {
            value = Null.Instance;
            return false;
        }
    }
}
