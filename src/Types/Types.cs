namespace Crisp.Types
{
    interface IType { }

    class TypeNumber : IType
    {
    }

    class TypeBoolean : IType
    {
    }

    class TypeString : IType
    {
    }

    class TypeNull : IType
    {
    }

    class TypeFunction : IType
    {
        public IType Argument { get; }
        public IType Result { get; }
        public TypeFunction(IType argument, IType result)
        {
            Argument = argument;
            Result = result;
        }
    }
}
