namespace Crisp
{
    class ObjectInteger : IObject
    {
        public long Value { get; }

        public ObjectInteger(long value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
