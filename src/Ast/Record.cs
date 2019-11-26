using System.Collections.Generic;

namespace Crisp.Ast
{
    class Record
    {
        public string Name { get; set; }

        public List<string> Variables { get; } = new List<string>();

        public Dictionary<string, Function> Functions { get; } = new Dictionary<string, Function>();

        public Record(string name)
        {
            Name = name;
        }
    }
}
