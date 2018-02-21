namespace Crisp.Ast
{
    class AssignmentVariable : IExpression
    {
        public Identifier Identifier { get; }

        public IExpression Value { get; }

        public AssignmentVariable(Identifier identifier, IExpression value)
        {
            Identifier = identifier;
            Value = value;
        }
    }
}
