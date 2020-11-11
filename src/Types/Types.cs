namespace Crisp.Types
{
    interface IType { }

    record TypeNumber : IType;

    record TypeBoolean : IType;

    record TypeString : IType;

    record TypeNull : IType;

    record TypeFunction(IType Argument, IType Result) : IType;
}
