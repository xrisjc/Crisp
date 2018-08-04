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

        public SymbolTag? Lookup(string symbol)
        {
            for (var st = this; st != null; st = st.Outer)
            {
                if (st.table.TryGetValue(symbol, out var tag))
                {
                    return tag;
                }
            }
            return null;
        }
    }
}
