namespace Crisp
{
    class ObjectNull : IObject
    {
        private ObjectNull()
        {
        }

        public override string ToString()
        {
            return "null";
        }

        public static ObjectNull Instance { get; } = new ObjectNull();
    }
}
