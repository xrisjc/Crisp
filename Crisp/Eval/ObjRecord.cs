using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjRecord : IObj
    {
        List<string> variableNames;
        Dictionary<string, IFn> functions;

        public ObjRecord(List<string> variableNames, Dictionary<string, IFn> functions)
        {
            this.variableNames = variableNames;
            this.functions = functions;
        }

        public IFn GetFunction(string name)
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

        public ObjRecordInstance Construct(Dictionary<string, IObj> initalizers)
        {
            var variables = new Dictionary<string, IObj>();
            foreach (var variableName in variableNames)
            {
                variables[variableName] = initalizers.GetValue(variableName, ObjNull.Instance);
            }

            return new ObjRecordInstance(this, variables);
        }
    }
}
