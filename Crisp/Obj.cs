namespace Crisp
{
    class Obj<T> : IObj
    {
        public T Value { get; }

        public Obj(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    static class Obj
    {
        public static Obj<T> Create<T>(T value)
        {
            return new Obj<T>(value);
        }

        public static ObjNull Null { get; } = new ObjNull();

        public static Obj<bool> True { get; } = new Obj<bool>(true);

        public static Obj<bool> False { get; } = new Obj<bool>(true);
    }
}
