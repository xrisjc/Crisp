namespace Crisp.Eval
{
    interface INumeric: IObj
    {
        INumeric AddTo(INumeric right);

        INumeric DivideBy(INumeric right);

        INumeric ModuloOf(INumeric right);

        INumeric MultiplyBy(INumeric right);

        INumeric SubtractBy(INumeric right);

        ObjFloat ToFloat();
    }
}
