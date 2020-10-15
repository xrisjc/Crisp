using System;
using System.Linq;
using Crisp.Ast;

namespace Crisp.Runtime
{
    class Interpreter : IExpressionVisitor<object>
    {
        public Environment Globals { get; }
        public Environment Environment { get; }

        public Interpreter(
            Environment globals,
            Environment environment)
        {
            Globals = globals;
            Environment = environment;
        }

        public Interpreter(Environment globals)
            : this(globals, globals)
        {
        }

        Interpreter Push()
            => new Interpreter(Globals, new Environment(Environment));

        bool IsTruthy(object obj)
            => obj switch { bool x => x, Null _ => false, _ => true };

        public object Evaluate(IExpression expression)
            => expression.Accept(this);

        public object Visit(AssignmentIdentifier ai)
        {
            var result = Evaluate(ai.Value);
            if (!Environment.Set(ai.Target.Name, result))
                throw new RuntimeErrorException(
                    ai.Target.Position,
                    $"Cannot assign, <{ai.Target.Name}> is unbound");
            return result;
        }

        public object Visit(Block block)
        {
            object result = new Null();
            {
                var interpreter = Push();
                foreach (var e in block.Body)
                    result = interpreter.Evaluate(e);
            }
            return result;
        }

        public object Visit(Call call)
        {
            var target = Evaluate(call.Target);

            if (target is Callable callable)
            {
                var args = from arg in call.Arguments
                           select Evaluate(arg);
                return callable(this, args.ToArray());
            }
            else
            {
                throw new RuntimeErrorException(
                    call.Position,
                    $"Cannot call non-callable object <{target}>.");
            }
        }

        public object Visit(Conditional c)
        {
            foreach (var (condition, consequence) in c.Branches)
            {
                if (IsTruthy(Evaluate(condition)))
                    return Evaluate(consequence);
            }

            if (c.ElseBlock != null)
                return Evaluate(c.ElseBlock);

            return new Null();
        }

        public object Visit(Ast.Function function)
        {
            var closure = Environment;

            Callable callable = (Interpreter interpreter, object[] arguments) =>
            {
                var environment = new Environment(closure);
                interpreter = new Interpreter(
                    interpreter.Globals,
                    environment);
                var parameters = function.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    object value;
                    if (i < arguments.Length)
                        value = arguments[i];
                    else
                        value = new Null();
                    
                    if (!environment.Create(parameters[i].Name, value))
                        throw new RuntimeErrorException(
                            parameters[i].Position,
                            $"Parameter {parameters[i].Name} already bound.");
                }
                return interpreter.Evaluate(function.Body);
            };

            return callable;
        }

        public object Visit(Identifier identifier)
            => Environment.Get(identifier.Name) ??
                    throw new RuntimeErrorException(
                        identifier.Position,
                        $"Identifier <{identifier.Name}> unbound");

        public object Visit(LiteralBool literalBool) => literalBool.Value;

        public object Visit(LiteralNull ln) => new Null();

        public object Visit(LiteralNumber number) => number.Value;

        public object Visit(LiteralString literalString) => literalString.Value;

        public object Visit(OperatorBinary op)
        {
            if (op.Tag == OperatorBinaryTag.And)
                return IsTruthy(Evaluate(op.Left)) &&
                    IsTruthy(Evaluate(op.Right));
            
            if (op.Tag == OperatorBinaryTag.Or)
                return IsTruthy(Evaluate(op.Left)) ||
                        IsTruthy(Evaluate(op.Right));

            var left = Evaluate(op.Left);
            var right = Evaluate(op.Right);

            if (op.Tag == OperatorBinaryTag.Eq)
                return left.Equals(right);
            else if (op.Tag == OperatorBinaryTag.Neq)
                return !left.Equals(right);

            return (op.Tag, left, right) switch
            {
                (OperatorBinaryTag.Add,  double l, double r) => l + r,
                (OperatorBinaryTag.Sub,  double l, double r) => l - r,
                (OperatorBinaryTag.Mul,  double l, double r) => l * r,
                (OperatorBinaryTag.Div,  double l, double r) => l / r,
                (OperatorBinaryTag.Mod,  double l, double r) => l % r,
                (OperatorBinaryTag.Lt,   double l, double r) => l < r,
                (OperatorBinaryTag.LtEq, double l, double r) => l <= r,
                (OperatorBinaryTag.Gt,   double l, double r) => l > r,
                (OperatorBinaryTag.GtEq, double l, double r) => l >= r,
                _ => throw new RuntimeErrorException(
                        op.Position,
                        $"Operator {op.Tag} cannot be applied to values " +
                        $"<{left}> and <{right}>"),
            };
        }

        public object Visit(OperatorUnary op)
        {
            var obj = Evaluate(op.Expression);

            if (op.Op == OperatorUnaryTag.Not)
                return !IsTruthy(obj);

            return (op.Op, obj) switch
            {
                (OperatorUnaryTag.Neg, double n) => -n,
                _ => throw new RuntimeErrorException(
                        op.Position,
                        $"Operator {op.Op} cannot be applied to values `{obj}`"),
            };
        }

        public object Visit(Let let)
        {
            var result = Evaluate(let.InitialValue);
            if (!Environment.Create(let.Name.Name, result))
                throw new RuntimeErrorException(
                    let.Name.Position,
                    $"Identifier <{let.Name.Name}> is already bound");
            return result;
        }

        public object Visit(While @while)
        {
            while (IsTruthy(Evaluate(@while.Guard)))
                Evaluate(@while.Body);
            return new Null();
        }

        public object Visit(Write write)
        {
            foreach (var e in write.Arguments)
                Console.Write(Evaluate(e));
            return new Null();
        }
    }
}