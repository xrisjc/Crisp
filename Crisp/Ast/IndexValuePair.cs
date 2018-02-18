namespace Crisp.Ast
{
    class IndexValuePair
    {
        public IExpression Index { get; }

        public IExpression Value { get; }

        public IndexValuePair(IExpression index, IExpression value)
        {
            Index = index;
            Value = value;
        }
    }
}
