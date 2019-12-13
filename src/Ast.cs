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

    class AssignmentIndex : IExpression
    {
        public Index Index { get; }
        public IExpression Value { get; }
        public AssignmentIndex(Index index, IExpression value)
        {
            Index = index;
            Value = value;
        }
    }

    class AssignmentRefinement : IExpression
    {
        public Refinement Refinement { get; }
        public IExpression Value { get; }
        public AssignmentRefinement(Refinement refinement, IExpression value)
        {
            Refinement = refinement;
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

    class Function : IExpression
    {
        public List<Identifier> Parameters { get; }

        public IExpression Body { get; }

        public Function(List<Identifier> parameters, IExpression body)
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

    class If : IExpression
    {
        public IExpression Condition { get; }

        public IExpression Consequence { get; }

        public IExpression Alternative { get; }

        public If(
            IExpression condition,
            IExpression consequence,
            IExpression alternative)
        {
            Condition = condition;
            Consequence = consequence;
            Alternative = alternative;
        }
    }

    class Index : IExpression
    {
        public Position Position { get; }
        public IExpression Target { get; }
        public IExpression Key { get; }
        public Index(Position position, IExpression target, IExpression key)
        {
            Position = position;
            Target = target;
            Key = key;
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

    class LiteralNull : IExpression { }

    class LiteralObject : IExpression
    {
        public List<(Identifier, IExpression)> Properties { get; }
        public LiteralObject(List<(Identifier, IExpression)> properties)
        {
            Properties = properties;
        }
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
        Beget,
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

    class Refinement : IExpression
    {
        public IExpression Target { get; }
        public Identifier Name { get; }
        public Refinement(IExpression target, Identifier name)
        {
            Target = target;
            Name = name;
        }
    }

    class Self : IExpression
    {
        public Position Position { get; }
        public Self(Position position)
        {
            Position = position;
        }
    }

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

    class While : IExpression
    {
        public IExpression Guard { get; }

        public IExpression Body { get; }

        public While(IExpression guard, IExpression body)
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
