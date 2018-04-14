using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecord
    {
        List<string> variableNames;
        Dictionary<string, ObjFn> functions;

        public ObjRecord(List<string> variableNames, Dictionary<string, ObjFn> functions)
        {
            this.variableNames = variableNames;
            this.functions = functions;
        }

        public ObjFn GetFunction(string name)
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

        public ObjRecordInstance Construct(Dictionary<string, dynamic> initalizers)
        {
            var variables = new Dictionary<string, dynamic>();
            foreach (var variableName in variableNames)
            {
                variables[variableName] = initalizers.GetValue(variableName, Null.Instance);
            }

            return new ObjRecordInstance(this, variables);
        }
    }
}
