namespace Crisp.Ast
{
    class ForIn : IExpression
    {
        public string VariableName { get; }

        public IExpression Sequence { get; }

        public IExpression Body { get; }

        public ForIn(string variableName, IExpression sequence, IExpression body)
        {
            VariableName = variableName;
            Sequence = sequence;
            Body = body;
        }
    }
}
