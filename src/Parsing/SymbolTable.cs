using System.Collections.Generic;

namespace Crisp.Parsing
{
    class SymbolTable
    {
        Dictionary<string, SymbolTag> table = new Dictionary<string, SymbolTag>();

        public SymbolTable Outer { get; }

        public SymbolTable(SymbolTable outer = null)
        {
            Outer = outer;
        }

        public bool Create(string symbol, SymbolTag info)
        {
            if (table.TryGetValue(symbol, out var existingInfo))
            {
                return false;
            }
            else
            {
                table.Add(symbol, info);
                return true;
            }
        }

        public SymbolTag? Lookup(string symbol)
        {
            for (var st = this; st != null; st = st.Outer)
            {
                if (st.table.TryGetValue(symbol, out var info))
                {
                    return info;
                }
            }
            return null;
        }

        public void Write()
        {
            for (var st = this; st != null; st = st.Outer)
            {
                System.Console.WriteLine("#");
                foreach (var item in st.table)
                {
                    System.Console.WriteLine($"<{item.Key}> = {item.Value}");
                }
            }
        }
    }
}
