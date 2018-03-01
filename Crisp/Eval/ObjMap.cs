using Crisp.Ast;
using System.Collections.Generic;
using System.Text;

namespace Crisp.Eval
{
    class ObjMap : IObj, IIndexable, IMemberGet
    {
        Dictionary<IObj, IObj> items = new Dictionary<IObj, IObj>();

        public ObjMap(Map map, Environment environment)
        {
            foreach (var (index, value) in map.Initializers)
            {
                var objIndex = index.Evaluate(environment);
                var objValue = value.Evaluate(environment);
                IndexSet(objIndex, objValue);
            }
        }

        public string Print()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var item in items)
            {
                sb.AppendLine($"[{item.Key.Print()}] = {item.Value.Print()}");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public IObj IndexGet(IObj index)
        {
            if (items.TryGetValue(index, out IObj value))
            {
                return value;
            }
            else
            {
                throw new RuntimeErrorException(
                    $"map does not contain index '{index.Print()}'");
            }
        }

        public IObj IndexSet(IObj index, IObj value)
        {
            items[index] = value;
            return value;
        }

        public (IObj, GetStatus) MemberGet(string name)
        {
            switch (name)
            {
                case "count":
                    return (new ObjInt(items.Count), GetStatus.Found);
                default:
                    return (ObjNull.Instance, GetStatus.NotFound);
            }
        }
    }
}
