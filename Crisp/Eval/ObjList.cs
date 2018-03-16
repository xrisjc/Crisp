using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjList : IObj, ILen, IIndexGet, IIndexSet
    {
        List<IObj> items = new List<IObj>();

        public ObjList(Ast.List list, Environment environment)
        {
            foreach (var initializer in list.Initializers)
            {
                var item = initializer.Evaluate(environment);
                items.Add(item);
            }
        }

        public IType Type => TypeList.Instance;

        public int Len => items.Count;

        public IObj IndexGet(IObj index)
        {
            switch (index)
            {
                case ObjInt i when 0 <= i.Value && i.Value < items.Count:
                    return items[i.Value];

                case ObjInt i:
                    throw new RuntimeErrorException($"" +
                        $"index {i.Value} is out of range");

                default:
                    throw new RuntimeErrorException(
                        "cannot index a list with a non-integer value");
            }
        }

        public IObj IndexSet(IObj index, IObj value)
        {
            switch (index)
            {
                case ObjInt i when 0 <= i.Value && i.Value < items.Count:
                    items[i.Value] = value;
                    return value;

                case ObjInt i:
                    throw new RuntimeErrorException($"" +
                        $"index {i.Value} is out of range");

                default:
                    throw new RuntimeErrorException(
                        "cannot index a list with a non-integer value");
            }
        }

        public IObj Push(IObj value)
        {
            items.Add(value);
            return value;
        }
    }
}
