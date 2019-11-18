namespace Crisp.Ast
{
    class Branch
    {
        public IExpression Condition { get; }

        public IExpression Consequence { get; }

        public Branch(IExpression condition, IExpression consequence)
        {
            Condition = condition;
            Consequence = consequence;
        }
    }
}
