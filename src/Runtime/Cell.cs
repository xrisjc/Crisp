using System;

namespace Crisp.Runtime
{
    class Null
    {
        public override string? ToString() => "null";
    }

    class Cell
    {
        public object Value { get; set; }
        public Cell(object value) { Value = value; }
        public Cell() : this(new Null()) { }
        public override string? ToString()
            => Value.ToString();
    }
}
