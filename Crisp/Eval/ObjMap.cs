using Crisp.Ast;
using System.Collections.Generic;
using System.Text;

namespace Crisp.Eval
{
    class ObjMap : IObj, IIndexable
    {
        Dictionary<IObj, IObj> items = new Dictionary<IObj, IObj>();

        public ObjMap(Map map, Environment environment)
        {
            foreach (var initializer in map.Initializers)
            {
                Set(initializer, environment);
            }
        }

        public IObj Get(IObj index)
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

        public IObj Set(IObj index, IObj value)
        {
            items[index] = value;
            return value;
        }

        public IObj Set(IndexValuePair indexValuePair, Environment environment)
        {
            var index = indexValuePair.Index.Evaluate(environment);
            var value = indexValuePair.Value.Evaluate(environment);
            return Set(index, value);
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
    }
}
