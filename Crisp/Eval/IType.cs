namespace Crisp.Eval
{
    interface IType
    {
    }

    class TypeType : IObj, IType
    {
        TypeType() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeType();
    }

    class TypeBool : IObj, IType
    {
        TypeBool() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeBool();
    }

    class TypeFloat : IObj, IType
    {
        TypeFloat() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeFloat();
    }

    class TypeInt : IObj, IType
    {
        TypeInt() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeInt();
    }

    class TypeFn : IObj, IType
    {
        TypeFn() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeFn();
    }

    class TypeList : IObj, IType
    {
        TypeList() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeList();
    }

    class TypeMap : IObj, IType
    {
        TypeMap() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeMap();
    }

    class TypeNull : IObj, IType
    {
        TypeNull() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeNull();
    }

    class TypeString : IObj, IType
    {
        TypeString() { }

        public IType Type => TypeType.Instance;

        public static IType Instance { get; } = new TypeString();
    }
}
