using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    interface IExpression
    {
    }

    record ExpressionPair(IExpression Head, IExpression Tail) : IExpression;

    record AssignmentIdentifier(Identifier Target, IExpression Value) : IExpression;

    record Block(IExpression Body) : IExpression;

    record Call(Position Position, IExpression Target, List<IExpression> Arguments) : IExpression;

    record Conditional(
        IExpression Condition,
        IExpression Consequence,
        IExpression Alternative)
        : IExpression;

    record Function(List<Identifier> Parameters, IExpression Body) : IExpression;

    record Identifier(Position Position, string Name) : IExpression;

    record LiteralBool(bool Value) :  IExpression;

    record LiteralNull() : IExpression;

    record LiteralNumber(double Value) : IExpression;

    record LiteralString(string Value) : IExpression;

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

    record Program(IExpression Body) : IExpression;

    record Let(Identifier Name, IExpression InitialValue) : IExpression;

    record While(IExpression Guard, IExpression Body) : IExpression;

    record Write(List<IExpression> Arguments) : IExpression;
}
