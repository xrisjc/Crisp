using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    interface IProgramItem
    {
    }

    interface IExpression : IProgramItem
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

    class Call : IExpression
    {
        public Position Position { get; }

        public string Name { get; }

        public List<IExpression> Arguments { get; }

        public Call(Position position, string name, List<IExpression> arguments)
        {
            Position = position;
            Name = name;
            Arguments = arguments;
        }
    }

    class Condition : IExpression
    {
        public List<Branch> Branches { get; }

        public Condition(List<Branch> branches)
        {
            Branches = branches;
        }
    }

    class Function : IProgramItem
    {
        public Identifier Name { get; }

        public List<Identifier> Parameters { get; }

        public Block Body { get; }

        public Function(
            Identifier name,
            List<Identifier> parameters,
            Block body)
        {
            Name = name;
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

    class Literal : IExpression
    {
        public static Literal True { get; } = new Literal(true);

        public static Literal False { get; } = new Literal(false);

        public object Value { get; }

        public Literal(object value)
        {
            Value = value;
        }
    }

    class LiteralNull : IExpression
    {
        public static LiteralNull Instance { get; } = new LiteralNull();

        private LiteralNull()
        {
        }
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
        public Dictionary<string, Function> Fns { get; } =
            new Dictionary<string, Function>();

        public List<IExpression> Expressions { get; } =
            new List<IExpression>();

        public List<IProgramItem> ProgramItems { get; } =
            new List<IProgramItem>();

        public void Add(Function function)
        {
            ProgramItems.Add(function);

            // TODO: Check for duplicate names?
            Fns.Add(function.Name.Name, function);
        }

        public void Add(IExpression expression)
        {
            ProgramItems.Add(expression);
            Expressions.Add(expression);
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
