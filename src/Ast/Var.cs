namespace Crisp.Ast
{
    class Var : IExpression
    {
        public Identifier Name { get; }

        public IExpression InitialValue { get; }

        public Var(Identifier name, IExpression initialValue)
        {
            Name = name;
            InitialValue = initialValue;
        }
    }
}
