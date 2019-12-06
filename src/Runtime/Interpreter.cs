using System;
using Crisp.Ast;

namespace Crisp.Runtime
{
    class Interpreter
    {
        public Environment Globals { get; }
        public Environment? Locals { get; }
        public Program Program { get; }

        public Interpreter(
            Environment globals,
            Environment? locals,
            Program program)
        {
            Globals = globals;
            Locals = locals;
            Program = program;
        }
        
        Interpreter PushLocals() =>
            new Interpreter(
                Globals,
                new Environment(Locals),
                Program);

        object? Get(Identifier identifier) =>
            Locals?.Get(identifier) ??
            Globals.Get(identifier);

        bool Set(string name, object value) =>
            (Locals?.Set(name, value) ?? false) ||
            Globals.Set(name, value);

        bool Create(string name, object value) =>
            (Locals?.Create(name, value) ?? false) ||
            Globals.Create(name, value);

        dynamic Evaluate(IExpression expression)
        {
            dynamic result;
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    result = Evaluate(ai.Value);
                    if (!Set(ai.Target.Name, result))
                        throw new RuntimeErrorException(
                            ai.Target.Position,
                            $"Cannot assign, <{ai.Target.Name}> is unbound");
                    break;

                case Block block:
                    result = Null.Instance;
                    {
                        var interpreter = PushLocals();
                        foreach (var e in block.Body)
                            result = interpreter.Evaluate(e);
                    }
                    break;

                case Call call
                when Program.Fns.TryGetValue(call.Name, out var fn):
                    if (fn.Parameters.Count != call.Arguments.Count)
                        throw new RuntimeErrorException(
                            call.Position,
                            "Arity mismatch");
                    
                    {
                        var env = new Environment();
                        for (var i = 0; i < call.Arguments.Count; i++)
                        {
                            var arg = call.Arguments[i];
                            var param = fn.Parameters[i];
                            var value = Evaluate(arg);
                            if (!env.Create(param.Name, value))
                                throw new RuntimeErrorException(
                                    call.Position,
                                    $"Parameter {param} already bound");
                        }

                        var interpreter = new Interpreter(Globals, env, Program);
                        result = interpreter.Evaluate(fn.Body);
                    }
                    break;

                case Call call:
                    throw new RuntimeErrorException(
                        call.Position,
                        $"No function or type named <{call.Name}>.");

                case Condition condition:
                    result = Null.Instance;
                    foreach (var b in condition.Branches)
                        if (IsTruthy(Evaluate(b.Condition)))
                        {
                            result = Evaluate(b.Consequence);
                            break;
                        }
                    break;

                case Identifier identifier:
                    result = Get(identifier) ??
                                throw new RuntimeErrorException(
                                    identifier.Position,
                                    $"Identifier <{identifier.Name}> unbound");
                    break;

                case Literal literal:
                    result = literal.Value;
                    break;

                case LiteralNull _:
                    result = Null.Instance;
                    break;

                case OperatorBinary op when op.Tag == OperatorBinaryTag.And:
                    result = IsTruthy(Evaluate(op.Left))
                                ? IsTruthy(Evaluate(op.Right))
                                : false;
                    break;

                case OperatorBinary op when op.Tag == OperatorBinaryTag.Or:
                    result = IsTruthy(Evaluate(op.Left))
                                ? true
                                : IsTruthy(Evaluate(op.Right));
                    break;

                case OperatorBinary op:
                    var left = Evaluate(op.Left);
                    var right = Evaluate(op.Right);
                    result = op.Tag switch
                    {
                        OperatorBinaryTag.Add when left is string && right is string => string.Concat(left, right),

                        OperatorBinaryTag.Add when CheckNumeric(left, right) => left + right,

                        OperatorBinaryTag.Sub when CheckNumeric(left, right) => left - right,

                        OperatorBinaryTag.Mul when CheckNumeric(left, right) => left * right,

                        OperatorBinaryTag.Div when CheckNumeric(left, right) => left / right,

                        OperatorBinaryTag.Mod when CheckNumeric(left, right) => left % right,

                        OperatorBinaryTag.Lt when CheckNumeric(left, right) => left < right,

                        OperatorBinaryTag.LtEq when CheckNumeric(left, right) => left <= right,

                        OperatorBinaryTag.Gt when CheckNumeric(left, right) => left > right,

                        OperatorBinaryTag.GtEq when CheckNumeric(left, right) => left >= right,

                        OperatorBinaryTag.Eq when CheckNumeric(left, right) =>
                            // If left is int and right is double then
                            // left.Equals(right) may not be the same as
                            // right.Equals(left).  I suppose it's something to
                            // do with calinging Int32's Equals.  == seems to
                            // work for numbers, though.
                            left == right,

                        OperatorBinaryTag.Eq => left.Equals(right),
                        OperatorBinaryTag.Neq => !left.Equals(right),

                        _ =>
                            throw new RuntimeErrorException(
                                op.Position,
                                $"Operator {op.Tag} cannot be applied to values " +
                                $"<{left}> and <{right}>"),
                    };
                    break;

                case OperatorUnary op:
                    left = Evaluate(op.Expression);
                    result = op.Op switch
                    {
                        OperatorUnaryTag.Neg when CheckNumeric(left) => -left,
                        OperatorUnaryTag.Not => IsFalsey(left),
                        _ =>
                            throw new RuntimeErrorException(
                                op.Position,
                                $"Operator {op.Op} cannot be applied to values <{left}>"),
                    };
                    break;

                case Var var:
                    result = Evaluate(var.InitialValue);
                    if (!Create(var.Name.Name, result))
                        throw new RuntimeErrorException(
                            var.Name.Position,
                            $"Identifier <{var.Name.Name}> is already bound");
                    break;

                case While @while:
                    while (IsTruthy(Evaluate(@while.Guard)))
                        Evaluate(@while.Body);
                    result = Null.Instance;
                    break;

                case Write write:
                    foreach (var e in write.Arguments)
                        Console.Write(Evaluate(e));
                    result = Null.Instance;
                    break;

                default:
                    throw new NotImplementedException(
                        $"Unimplemented {expression.GetType()}");
            }

            return result;
        }

        public static object Run(Program program, Environment globals)
        {
            var interpreter = new Interpreter(globals, null, program);
            object result = Null.Instance;
            foreach (var expr in program.Expressions)
                result = interpreter.Evaluate(expr);
            return result;
        }

        static bool IsFalsey(dynamic x) =>
            x.Equals(false) || ReferenceEquals(x, Null.Instance);

        static bool IsTruthy(dynamic x) => !IsFalsey(x);

        static bool CheckNumeric(dynamic x) => x is int || x is double;

        static bool CheckNumeric(dynamic left, dynamic right) =>
            CheckNumeric(left) && CheckNumeric(right);
    }
}