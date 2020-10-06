using Crisp.Parsing;
using System.Collections.Generic;

namespace Crisp.Ast
{
    interface IExpression
    {
        T Accept<T>(IExpressionVisitor<T> visitor);
    }

    interface IExpressionVisitor<T>
    {
        T Visit(AssignmentIdentifier ai);
        T Visit(AssignmentIndex ai);
        T Visit(AssignmentRefinement ar);
        T Visit(Block b);
        T Visit(Call c);
        T Visit(Create c);
        T Visit(Conditional c);
        T Visit(Function f);
        T Visit(Identifier i);
        T Visit(Index i);
        T Visit(Let v);
        T Visit(LiteralBool lb);
        T Visit(LiteralNull ln);
        T Visit(LiteralNumber ln);
        T Visit(LiteralString ls);
        T Visit(LiteralList ll);
        T Visit(OperatorBinary ob);
        T Visit(OperatorUnary ou);
        T Visit(Refinement r);
        T Visit(Self s);
        T Visit(While w);
        T Visit(Write w);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class Block : IExpression
    {
        public List<IExpression> Body { get; }
        public Block(List<IExpression> body)
        {
            Body = body;
        }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class Create : IExpression
    {
        public IExpression Prototype { get; }
        public Create(IExpression prototype)
        {
            Prototype = prototype;
        }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    abstract class Literal<T>
    {
        public T Value { get; }
        public Literal(T value) { Value = value; }
    }

    class LiteralBool : Literal<bool>, IExpression
    {
        public LiteralBool(bool value) : base(value) { }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class LiteralNull : IExpression
    {
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class LiteralNumber : Literal<double>, IExpression
    {
        public LiteralNumber(double value) : base(value) { }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class LiteralString : Literal<string>, IExpression
    {
        public LiteralString(string value) : base(value) { }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class LiteralList : IExpression
    {
        public List<IExpression> Items { get; set; }
        public LiteralList(List<IExpression> items)
        {
            Items = items;
        }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        Is,
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class Self : IExpression
    {
        public Position Position { get; }
        public Self(Position position)
        {
            Position = position;
        }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
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
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }

    class Write : IExpression
    {
        public List<IExpression> Arguments { get; }
        public Write(List<IExpression> arguments)
        {
            Arguments = arguments;
        }
        public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
    }
}
