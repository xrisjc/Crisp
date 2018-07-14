using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Record
    {
        List<string> variableNames;
        Dictionary<string, Function> functions;

        public Record(List<string> variableNames, Dictionary<string, Function> functions)
        {
            this.variableNames = variableNames;
            this.functions = functions;
        }

        public bool GetInstanceMethod(string name, out Function method)
        {
            return functions.TryGetValue(name, out method);
        }

        public RecordInstance Construct(Dictionary<string, dynamic> initalizers)
        {
            var variables = new Dictionary<string, dynamic>();
            foreach (var variableName in variableNames)
            {
                variables[variableName] = initalizers.GetValue(variableName, Null.Instance);
            }

            return new RecordInstance(this, variables);
        }
    }
}
