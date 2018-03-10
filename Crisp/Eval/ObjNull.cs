namespace Crisp.Eval
{
    class ObjNull : IObj
    {
        private ObjNull() { }

        public static IObj Instance { get; } = new ObjNull();

        public override string ToString()
        {
            return "null";
        }
    }
}
