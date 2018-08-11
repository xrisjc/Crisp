namespace Crisp.Runtime
{
    class Null
    {
        private Null() { }

        public static Null Instance { get; } = new Null();

        public override string ToString() => "null";
    }
}
