namespace Crisp
{
    interface IExpression
    {
        IObj Evaluate(Environment environment);
    }
}
