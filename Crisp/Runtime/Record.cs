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

        public Function GetFunction(string name)
        {
            if (functions.TryGetValue(name, out var function))
            {
                return function;
            }
            else
            {
                throw new RuntimeErrorException($"cannot find function {name}");
            }
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
