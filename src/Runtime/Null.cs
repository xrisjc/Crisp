namespace Crisp.Runtime
{
    class Null
    {
        public override string ToString() => "null";
        public override bool Equals(object? obj) => obj is Null;
        public override int GetHashCode() => 0;
    }
}