namespace Crisp.Ast
{
    class AssignmentIndexing : IExpression
    {
        public Indexing Target { get; }

        public IExpression Value { get; }

        public AssignmentIndexing(Indexing target, IExpression value)
        {
            Target = target;
            Value = value;
        }
    }
}
