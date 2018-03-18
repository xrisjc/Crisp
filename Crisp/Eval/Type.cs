using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.Eval
{
    class Type : IObj
    {
        readonly static Type type = new Type("Type");

        public string Name { get; }

        public Type ObjType => type;

        public Type(string name)
        {
            Name = name;
        }

        public static Type Bool { get; } = new Type("Bool");

        public static Type Float { get; } = new Type("Float");

        public static Type Int { get; } = new Type("Int");

        public static Type Fn { get; } = new Type("Fn");

        public static Type List { get; } = new Type("List");

        public static Type Map { get; } = new Type("Map");

        public static Type Null { get; } = new Type("Null");

        public static Type String { get; } = new Type("String");
    }
}
