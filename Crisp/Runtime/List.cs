using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Runtime
{
    class List : Entity, IEnumerable<object>
    {
        List<object> list;

        public List(List<object> items)
        {
            this.list = items;
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

        public override bool SendMessage(string name, Evaluator evaluator)
        {
            dynamic arity = evaluator.Pop();

            // Ignore "this" on the stack.
            evaluator.Pop();
            arity--;

            var args = new object[arity];
            for (var i = 0; i < args.Length; i++)
            {
                args[args.Length - i - 1] = evaluator.Pop();
            }
            

            switch (name)
            {
                case "push":
                    list.AddRange(args);
                    evaluator.Push(this);
                    return true;
            }

            return base.SendMessage(name, evaluator);
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
