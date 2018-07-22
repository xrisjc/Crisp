using Crisp.Runtime;

namespace Crisp.Ast
{
    interface IExpression
    {
        void Accept(IExpressionVisitor visitor);
    }
}
