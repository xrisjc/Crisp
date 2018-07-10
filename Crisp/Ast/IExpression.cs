using Crisp.Eval;

namespace Crisp.Ast
{
    interface IExpression
    {
        object Evaluate(Environment environment);
    }
}
