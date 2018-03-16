using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjMap : IObj, ILen, IIndexGet, IIndexSet
    {
        Dictionary<IObj, IObj> items = new Dictionary<IObj, IObj>();

        public int Len => items.Count;

        public ObjMap(Map map, Environment environment)
        {
            foreach (var (index, value) in map.Initializers)
            {
                var objIndex = index.Evaluate(environment);
                var objValue = value.Evaluate(environment);
                IndexSet(objIndex, objValue);
            }
        }

        public IType Type => TypeMap.Instance;

        public IObj IndexGet(IObj index)
        {
            if (items.TryGetValue(index, out IObj value))
            {
                return value;
            }
            else
            {
                throw new RuntimeErrorException(
                    $"map does not contain index '{index}'");
            }
        }

        public IObj IndexSet(IObj index, IObj value)
        {
            items[index] = value;
            return value;
        }
    }
}
