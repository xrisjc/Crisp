namespace Crisp.Eval
{
    class ObjNull : IObj
    {
        private ObjNull() { }

        public static ObjNull Instance { get; } = new ObjNull();

        public string Print()
        {
            return "null";
        }
    }
}
