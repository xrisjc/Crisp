using System.Collections.Generic;

namespace Crisp.Parsing
{
    class SymbolTable
    {
        Dictionary<string, SymbolTag> table = new Dictionary<string, SymbolTag>();

        public SymbolTable Outer { get; }

        public SymbolTable(SymbolTable outer)
        {
            Outer = outer;
        }

        public bool Create(string symbol, SymbolTag tag)
        {
            if (table.TryGetValue(symbol, out var existingTag))
            {
                return false;
            }
            else
            {
                table.Add(symbol, tag);
                return true;
            }
        }
    }
}
