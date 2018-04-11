namespace Crisp.Ast
{
    class Len : IExpression
    {
        public IExpression Expression { get; }

        public Len(IExpression expression)
        {
            Expression = expression;
        }
    }
}
