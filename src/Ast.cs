using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    interface IExpression
    {
    }

    class AssignmentIdentifier : IExpression
    {
        public Identifier Target { get; }
        public IExpression Value { get; }
        public AssignmentIdentifier(Identifier target, IExpression value)
        {
            Target = target;
            Value = value;
        }
    }

    class Block : IExpression
    {
        public List<IExpression> Body { get; }
        public Block(List<IExpression> body)
        {
            Body = body;
        }
    }

    class Call : IExpression
    {
        public Position Position { get; }
        public IExpression Target { get; }
        public List<IExpression> Arguments { get; }
        public Call(
            Position position,
            IExpression target,
            List<IExpression> arguments)
        {
            Position = position;
            Target = target;
            Arguments = arguments;
        }
    }

    class Conditional : IExpression
    {
        public List<(IExpression, Block)> Branches { get; }
        public Block? ElseBlock { get; }
        public Conditional(List<(IExpression, Block)> branches, Block? elseBlock)
        {
            Branches = branches;
            ElseBlock = elseBlock;
        }
    }

    class Function : IExpression
    {
        public List<Identifier> Parameters { get; }
        public Block Body { get; }
        public Function(List<Identifier> parameters, Block body)
        {
            Parameters = parameters;
            Body = body;
        }
    }

    class Identifier : IExpression
    {
        public Position Position { get; }
        public string Name { get; }
        public Identifier(Position position, string name)
        {
            Position = position;
            Name = name;
        }
    }

    abstract class Literal<T>
    {
        public T Value { get; }
        public Literal(T value) { Value = value; }
    }

    class LiteralBool : Literal<bool>, IExpression
    {
        public LiteralBool(bool value) : base(value) { }
    }

    class LiteralNull : IExpression
    {
    }

    class LiteralNumber : Literal<double>, IExpression
    {
        public LiteralNumber(double value) : base(value) { }
    }

    class LiteralString : Literal<string>, IExpression
    {
        public LiteralString(string value) : base(value) { }
    }

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

    class OperatorBinary : IExpression
    {
        public Position Position { get; }
        public OperatorBinaryTag Tag { get; }
        public IExpression Left { get; }
        public IExpression Right { get; }
        public OperatorBinary(Position position, OperatorBinaryTag tag, IExpression left, IExpression right)
        {
            Position = position;
            Tag = tag;
            Left = left;
            Right = right;
        }
    }

    enum OperatorUnaryTag
    {
        Neg,
        Not,
    }

    class OperatorUnary : IExpression
    {
        public Position Position { get; }
        public OperatorUnaryTag Op { get; }
        public IExpression Expression { get; }
        public OperatorUnary(Position position, OperatorUnaryTag op, IExpression expression)
        {
            Position = position;
            Op = op;
            Expression = expression;
        }
    }

    class Program
    {
        public List<IExpression> Expressions { get; }
        public Program(List<IExpression> expressions)
        {
            Expressions = expressions;
        }
    }

    class Let : IExpression
    {
        public Identifier Name { get; }
        public IExpression InitialValue { get; }
        public Let(Identifier name, IExpression initialValue)
        {
            Name = name;
            InitialValue = initialValue;
        }
    }

    class While : IExpression
    {
        public IExpression Guard { get; }
        public Block Body { get; }
        public While(IExpression guard, Block body)
        {
            Guard = guard;
            Body = body;
        }
    }

    class Write : IExpression
    {
        public List<IExpression> Arguments { get; }
        public Write(List<IExpression> arguments)
        {
            Arguments = arguments;
        }
    }
}
