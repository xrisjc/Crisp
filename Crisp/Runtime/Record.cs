using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Record
    {
        public List<string> VariableNames { get; }

        public Dictionary<string, Function> Functions { get; }

        public Record(List<string> variableNames, Dictionary<string, Function> functions)
        {
            VariableNames = variableNames;
            Functions = functions;
        }
    }
}
