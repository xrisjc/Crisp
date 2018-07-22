namespace Crisp.Ast
{
    class Indexing : IExpression
    {
        public IExpression Indexable { get; }

        public IExpression Index { get; }

        public Indexing(IExpression indexable, IExpression index)
        {
            Indexable = indexable;
            Index = index;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
