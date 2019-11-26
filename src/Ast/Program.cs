using System.Collections.Generic;

namespace Crisp.Ast
{
    class Program
    {
        public Dictionary<string, Record> Types { get; } = new Dictionary<string, Record>();

        public Dictionary<string, Function> Fns { get; } = new Dictionary<string, Function>();

        public List<IExpression> Expressions { get; } = new List<IExpression>();
    }
}
