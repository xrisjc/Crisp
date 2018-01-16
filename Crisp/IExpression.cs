namespace Crisp
{
    interface IExpression : IAst
    {
        IObj Evaluate(Environment environment);
    }
}
