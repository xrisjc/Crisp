using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Runtime
{
    class List : IEntity, IEnumerable<object>
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

        public bool GetAttribute(string name, out object value)
        {
            switch (name)
            {
                case "length":
                    value = Count;
                    return true;

                default:
                    value = Null.Instance;
                    return false;
            }
        }

        public bool SetAttribute(string name, object value)
        {
            return false;
        }

        public bool SendMessage(string name, List<object> arguments, out object value)
        {
            value = this;
            switch (name)
            {
                case "push":
                    return Push(arguments);

                default:
                    return false;
            }
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
