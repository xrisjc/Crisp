using Crisp.Eval;

namespace Crisp.Ast
{
    class Branch : IExpression
    {
        public IExpression Condition { get; }

        public IExpression Consequence { get; }

        public IExpression Alternative { get; }

        public Branch(IExpression condition, IExpression consequence, IExpression alternative)
        {
            Condition = condition;
            Consequence = consequence;
            Alternative = alternative;
        }

        public object Evaluate(Environment environment)
        {
            if (Eval.Utility.IsTrue(Condition.Evaluate(environment)))
            {
                return Consequence.Evaluate(environment);
            }
            else
            {
                return Alternative.Evaluate(environment);
            }
        }
    }
}
