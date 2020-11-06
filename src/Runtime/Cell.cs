using System;

namespace Crisp.Runtime
{
    class Cell : IEquatable<Cell>
    {
        public object Value { get; set; }
        public Cell(object value) { Value = value; }
        public Cell() : this(new Null()) { }
        public bool IsTruthy()
            => Value switch { bool x => x, Null _ => false, _ => true };
        public bool Equals(Cell? other)
            => other != null && Value.Equals(other.Value);
        public override bool Equals(object? other)
            => Equals(other as Cell);
        public override int GetHashCode()
            => Value.GetHashCode();
        public override string? ToString()
            => Value.ToString();
    }
}
