using System;
using System.Linq;
using Crisp.Ast;

namespace Crisp.Runtime
{
    class Interpreter
    {
        public Environment Environment { get; }
        public Program Program { get; }
        public RecordInstance This { get; }

        public Interpreter(
            Environment environment,
            Program program,
            RecordInstance @this)
        {
            Environment = environment;
            Program = program;
            This = @this;
        }

        public Interpreter PushEnvironment() =>
            new Interpreter(new Environment(Environment), Program, This);

        public Interpreter PushThis(RecordInstance @this) =>
            new Interpreter(Environment, Program, @this);

        public dynamic Evaluate(IExpression expression)
        {
            dynamic result;
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    result = Evaluate(ai.Value);
                    if (!Environment.Set(ai.Target.Name, result))
                        throw new RuntimeErrorException(
                            ai.Target.Position,
                            $"Identifier <{ai.Target.Name}> is unbound");
                    break;

                case AttributeAccess aa
                when Evaluate(aa.Entity) is RecordInstance entity1:
                    // TODO: Need positions for all the runtime errors.
                    if (!entity1.GetAttribute(aa.Name, out result))
                        throw new RuntimeErrorException(
                            $"Attribute {aa.Name} not found");
                    break;

                case AttributeAccess aa:
                    // TODO: Need positions for all the runtime errors.
                    throw new RuntimeErrorException(
                        "Attribute access on non entity object");

                case AttributeAssignment aa
                when Evaluate(aa.Entity) is RecordInstance entity2:
                    result = Evaluate(aa.Value);
                    if (!entity2.SetAttribute(aa.Name, result))
                        // TODO: Need position!
                        throw new RuntimeErrorException(
                            $"attritue {aa.Name} not found.");
                    break;

                case AttributeAssignment aa:
                    // TODO: Need position!            
                    throw new RuntimeErrorException(
                        aa.Position,
                        "Attribute assignment on non entity object");

                case Block block:
                    result = Null.Instance;
                    {
                        var interpreter = PushEnvironment();
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
                            if (!env.Create(param, value))
                                throw new RuntimeErrorException(
                                    $"{param} already bound");
                        }

                        var interpreter = new Interpreter(env, Program, This);
                        result = interpreter.Evaluate(new Block(fn.Body));
                    }
                    break;

                case Call call
                when Program.Types.TryGetValue(call.Name, out var type):
                    if (call.Arguments.Count > 0)
                        throw new RuntimeErrorException(
                            $"Record constructor {call.Name} must have no arguments");

                    result = new RecordInstance(
                        type.Name,
                        type.Variables.ToDictionary(
                            name => name,
                            name => (object)Null.Instance));
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
                    if (!Environment.Get(identifier.Name, out result))
                        throw new RuntimeErrorException(
                            identifier.Position,
                            $"Identifier <{identifier.Name}> unbound");
                    break;

                case Literal literal:
                    result = literal.Value ?? Null.Instance;
                    break;

                case MessageSend ms:
                    if (Evaluate(ms.EntityExpr) is RecordInstance @this)
                    {
                        // TODO: There is no error checking here!
                        var record = Program.Types[@this.RecordName];
                        var fn = record.Functions[ms.Name];

                        if (fn.Parameters.Count != ms.ArgumentExprs.Count)
                            throw new RuntimeErrorException(
                                ms.Position,
                                "Arity mismatch");

                        var env = new Environment();
                        for (var i = 0; i < ms.ArgumentExprs.Count; i++)
                        {
                            var arg = ms.ArgumentExprs[i];
                            var param = fn.Parameters[i];
                            var value = Evaluate(arg);
                            if (!env.Create(param, value))
                                // TODO: No position for the parameter!
                                throw new RuntimeErrorException(
                                    $"{param} already bound");
                        }

                        var interpreter = new Interpreter(env, Program, @this);
                        result = interpreter.Evaluate(new Block(fn.Body));
                    }
                    else
                        throw new RuntimeErrorException(
                            ms.Position,
                            "Message sent to non entity object");
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

                case This _:
                    result = (object)This ?? Null.Instance;
                    break;

                case Var var:
                    result = Evaluate(var.InitialValue);
                    if (!Environment.Create(var.Name, result))
                        throw new RuntimeErrorException(
                            $"Identifier <{var.Name}> is already bound");
                    break;

                case While @while:
                    while (IsTruthy(Evaluate(@while.Guard)))
                        Evaluate(new Block(@while.Body));
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

        public static object Run(Program program, Environment environment)
        {
            var interpreter = new Interpreter(environment, program, null);
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
