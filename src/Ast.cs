using Crisp.Parsing;

namespace Crisp.Ast
{
    interface IExpression
    {
    }

    record AssignmentIdentifier(
        Identifier Target,
        IExpression Value)
        : IExpression;

    record Conditional(
        IExpression Condition,
        IExpression Consequence,
        IExpression Alternative)
        : IExpression;

    record ExpressionPair(
        IExpression Head,
        IExpression Tail)
        : IExpression;

    record Function(
        IExpression Body)
        : IExpression;

    record FunctionCall(
        Position Position,
        IExpression Target,
        IExpression Argument)
        : IExpression;

    record Identifier(
        int Depth)
        : IExpression;

    record Let(
        IExpression InitialValue,
        IExpression Body)
        : IExpression;

    record LetRec(
        IExpression Callable,
        IExpression Body)
        : IExpression;

    record LiteralBool(
        bool Value)
        : IExpression;

    record LiteralNull()
        : IExpression;

    record LiteralNumber(
        double Value)
        : IExpression;

    record LiteralString(
        string Value)
        : IExpression;

    enum OperatorBinaryTag
    {
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Lt,
        LtEq,
        Gt,
        GtEq,
        And,
        Or,
        Eq,
        Neq,
    }

    record OperatorBinary(
        Position Position,
        OperatorBinaryTag Tag,
        IExpression Left,
        IExpression Right)
        : IExpression;

    enum OperatorUnaryTag
    {
        Neg,
        Not,
    }

    record OperatorUnary(
        Position Position,
        OperatorUnaryTag Op,
        IExpression Expression)
        : IExpression;

    record While(
        IExpression Guard,
        IExpression Body)
        : IExpression;

    record Write(
        IExpression Value)
        : IExpression;
}
