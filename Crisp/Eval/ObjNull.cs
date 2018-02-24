namespace Crisp.Eval
{
    class ObjNull : IObj
    {
        private ObjNull() { }

        public static ObjNull Instance { get; } = new ObjNull();

        public override string ToString()
        {
            return "null";
        }

        public string Print()
        {
            return ToString();
        }
    }
}
