namespace Crisp.Ast
{
    class For : IExpression
    {
        public string VariableName { get; }

        public IExpression Start { get; }

        public IExpression End { get; }

        public IExpression Body { get; }

        public For(string variableName, IExpression start, IExpression end, IExpression body)
        {
            VariableName = variableName;
            Start = start;
            End = end;
            Body = body;
        }
    }
}
