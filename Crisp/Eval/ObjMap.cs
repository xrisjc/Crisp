using Crisp.Ast;
using System.Collections.Generic;
using System.Text;

namespace Crisp.Eval
{
    class ObjMap : IObj, IIndexable
    {
        Dictionary<IObj, IObj> items = new Dictionary<IObj, IObj>();

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

        public void Set(IObj index, IObj value)
        {
            items[index] = value;
        }

        public void Set(IndexValuePair indexValuePair, Environment environment)
        {
            var index = indexValuePair.Index.Evaluate(environment);
            var value = indexValuePair.Value.Evaluate(environment);
            Set(index, value);
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
