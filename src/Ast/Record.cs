using System.Collections.Generic;

namespace Crisp.Ast
{
    class Record : IProgramItem
    {
        public string Name { get; }

        public List<Identifier> Variables { get; } =
            new List<Identifier>();

        public Dictionary<string, Function> Functions { get; } =
            new Dictionary<string, Function>();

        public Record(string name)
        {
            Name = name;
        }
    }
}
