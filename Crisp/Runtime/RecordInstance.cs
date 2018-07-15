using System.Collections.Generic;
using static Crisp.Runtime.Utility;

namespace Crisp.Runtime
{
    class RecordInstance : Entity
    {
        Record record;
        Dictionary<string, dynamic> variables;

        public RecordInstance(Record record, Dictionary<string, dynamic> variables)
        {
            this.record = record;
            this.variables = variables;
        }

        public override bool GetAttribute(string name, out dynamic value)
        {
            if (variables.TryGetValue(name, out value))
            {
                return true;
            }
            else
            {
                value = Null.Instance;
                return false;
            }
        }

        public override bool SetAttribute(string name, dynamic value)
        {
            if (variables.ContainsKey(name))
            {
                variables[name] = value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SendMessage(string name, List<object> arguments, out object value)
        {
            if (!record.GetInstanceMethod(name, out var method))
            {
                value = Null.Instance;
                return false;
            }

            if (method.Parameters.Count != arguments.Count + 1) // + 1 for "this" argument
            {
                value = Null.Instance;
                return false;
            }

            arguments.Add(this); // the "this" argument is at the end

            var localEnvironment = new Environment(method.Environment);
            Bind(method.Parameters, arguments, localEnvironment);

            value = method.Body.Evaluate(localEnvironment);
            return true;
        }
    }
}
