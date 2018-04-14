namespace Crisp.Eval
{
    class Null
    {
        private Null() { }

        public static Null Instance { get; } = new Null();
    }
}
