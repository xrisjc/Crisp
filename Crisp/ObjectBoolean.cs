namespace Crisp
{
    class ObjectBoolean : IObject
    {
        public bool Value { get; }

        public ObjectBoolean(bool value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value ? "true" : "false";
        }
    }
}
