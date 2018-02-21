namespace Crisp.Ast
{
    class AssignmentIndex : IExpression
    {
        public Indexing Index { get; }

        public IExpression Value { get; }

        public AssignmentIndex(Indexing index, IExpression value)
        {
            Index = index;
            Value = value;
        }
    }
}
