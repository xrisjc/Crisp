namespace Crisp
{
    interface IExpression : IAst
    {
        IObject Evaluate(Environment environment);
    }
}
