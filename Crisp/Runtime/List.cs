using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Runtime
{
    class List : Entity, IEnumerable<object>
    {
        List<object> list;

        public List(IEnumerable<object> items)
        {
            this.list = items.ToList();
        }

        public int Count => list.Count;

        public object this[int i]
        {
            get { return list[i]; }
            set { list[i] = value; }
        }

        public override bool GetAttribute(string name, out object value)
        {
            switch (name)
            {
                case "length":
                    value = Count;
                    return true;
            }

            return base.GetAttribute(name, out value);
        }

        public override bool SendMessage(string name, List<object> arguments, out object value)
        {
            switch (name)
            {
                case "push":
                    value = this;
                    return Push(arguments);
            }

            return base.SendMessage(name, arguments, out value);
        }

        bool Push(List<object> arguments)
        {
            if (arguments.Count == 0)
            {
                return false;
            }

            list.AddRange(arguments);
            return true;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
