using System.Collections.Generic;

namespace Crisp.Ast
{
    class Program
    {
        public Dictionary<string, Record> Types { get; } =
            new Dictionary<string, Record>();

        public Dictionary<string, Function> Fns { get; } =
            new Dictionary<string, Function>();

        public List<IExpression> Expressions { get; } =
            new List<IExpression>();

        public List<IProgramItem> ProgramItems { get; } =
            new List<IProgramItem>();

        public void Add(Record record)
        {
            ProgramItems.Add(record);

            // TODO: Check for duplicate names?
            Types.Add(record.Name, record);
        }

        public void Add(Function function)
        {
            ProgramItems.Add(function);

            // TODO: Check for duplicate names?
            Fns.Add(function.Name.Name, function);
        }

        public void Add(IExpression expression)
        {
            ProgramItems.Add(expression);
            Expressions.Add(expression);
        }
    }
}
