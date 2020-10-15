using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    interface IExpression
    {
    }

    record AssignmentIdentifier(Identifier Target, IExpression Value) : IExpression;

    record Block(List<IExpression> Body) : IExpression;

    record Call(Position Position, IExpression Target, List<IExpression> Arguments) : IExpression;

    record Conditional(List<(IExpression, Block)> Branches, Block? ElseBlock) : IExpression;

    record Function(List<Identifier> Parameters, Block Body) : IExpression;

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

    record Program(List<IExpression> Expressions);

    record Let(Identifier Name, IExpression InitialValue) : IExpression;

    record While(IExpression Guard, Block Body) : IExpression;

    record Write(List<IExpression> Arguments) : IExpression;
}
