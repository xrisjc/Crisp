using Crisp.Runtime;

namespace Crisp.Ast
{
    interface IExpression
    {
        object Evaluate(Environment environment);
    }
}
