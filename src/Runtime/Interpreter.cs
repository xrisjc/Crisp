using System;
using Crisp.Ast;

namespace Crisp.Runtime
{
    class Interpreter
    {
        public Environment Globals { get; }
        public Environment Environment { get; }

        public Interpreter(Environment globals, Environment environment)
        {
            Globals = globals;
            Environment = environment;
        }

        public Interpreter(Environment globals)
            : this(globals, globals)
        {
        }

        public CrispObject Evaluate(IExpression expression)
        {
            CrispObject result;
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    result = Evaluate(ai.Value);
                    if (!Environment.Set(ai.Target.Name, result))
                        throw new RuntimeErrorException(
                            ai.Target.Position,
                            $"Cannot assign, <{ai.Target.Name}> is unbound");
                    break;

                case AssignmentIndex ai:
                    {
                        var target = Evaluate(ai.Index.Target);
                        var key = Evaluate(ai.Index.Key);
                        var value = Evaluate(ai.Value);
                        target.Set(key, value);
                        result = value;
                    }
                    break;

                case AssignmentRefinement ar:
                    {
                        var target = Evaluate(ar.Refinement.Target);
                        var key = ar.Refinement.Name.Name;
                        var value = Evaluate(ar.Value);
                        target.Set(key, value);
                        result = value;
                    }
                    break;

                case Block block:
                    result = new ObjectNull();
                    {
                        var interpreter = new Interpreter(
                            Globals,
                            new Environment(Environment));
                        foreach (var e in block.Body)
                            result = interpreter.Evaluate(e);
                    }
                    break;

                case Call call:
                    if (Evaluate(call.Target) is ObjectFunction fn)
                        if (fn.Value.Parameters.Count == call.Arguments.Count)
                        {
                            var env = new Environment(Globals);
                            for (var i = 0; i < call.Arguments.Count; i++)
                            {
                                var arg = call.Arguments[i];
                                var param = fn.Value.Parameters[i];
                                var value = Evaluate(arg);
                                if (!env.Create(param.Name, value))
                                    throw new RuntimeErrorException(
                                        param.Position,
                                        $"Parameter {param} already bound");
                            }

                            var interpreter = new Interpreter(Globals, env);
                            result = interpreter.Evaluate(fn.Value.Body);
                        }
                        else
                            throw new RuntimeErrorException(
                                call.Position,
                                "Arity mismatch");
                    else
                        throw new RuntimeErrorException(
                            call.Position,
                            "Cannot call non-callable object.");
                    break;

                case If @if:
                    if (Evaluate(@if.Condition).IsTruthy())
                        result = Evaluate(@if.Consequence);
                    else
                        result = Evaluate(@if.Alternative);
                    break;

                case Ast.Index index:
                    {
                        var target = Evaluate(index.Target);
                        var key = Evaluate(index.Key);
                        if (target.Get(key) is CrispObject value)
                            result = value;
                        else
                            throw new RuntimeErrorException(
                                index.Position,
                                $"Key <{key}> does not exist in indexed object.");
                    }
                    break;

                case Function function:
                    result = new ObjectFunction(function);
                    break;

                case Identifier identifier:
                    result = Environment.Get(identifier.Name) ??
                                throw new RuntimeErrorException(
                                    identifier.Position,
                                    $"Identifier <{identifier.Name}> unbound");
                    break;

                case LiteralBool literalBool:
                    result = literalBool.Value;
                    break;

                case LiteralNull _:
                    result = new ObjectNull();
                    break;

                case LiteralNumber number:
                    result = new ObjectNumber(number.Value);
                    break;

                case LiteralObject literalObject:
                    result = new CrispObject();
                    foreach (var (key, value) in literalObject.Properties)
                        result.Set(key.Name, Evaluate(value));
                    break;

                case LiteralString literalString:
                    result = literalString.Value;
                    break;

                case OperatorBinary op when op.Tag == OperatorBinaryTag.And:
                    {
                        var leftResult = Evaluate(op.Left).IsTruthy();
                        if (leftResult)
                            result = Evaluate(op.Right).IsTruthy();
                        else
                            result = leftResult;
                    }
                    break;

                case OperatorBinary op when op.Tag == OperatorBinaryTag.Or:
                    {
                        var leftResult = Evaluate(op.Left).IsTruthy();
                        if (leftResult)
                            result = leftResult;
                        else
                            result = Evaluate(op.Right).IsTruthy();
                    }
                    break;

                case OperatorBinary op:
                    var left = Evaluate(op.Left);
                    var right = Evaluate(op.Right);
                    result = op.Tag switch
                    {
                        OperatorBinaryTag.Add when left is ObjectNumber l && right is ObjectNumber r => l + r,
                        OperatorBinaryTag.Sub when left is ObjectNumber l && right is ObjectNumber r => l - r,
                        OperatorBinaryTag.Mul when left is ObjectNumber l && right is ObjectNumber r => l * r,
                        OperatorBinaryTag.Div when left is ObjectNumber l && right is ObjectNumber r => l / r,
                        OperatorBinaryTag.Mod when left is ObjectNumber l && right is ObjectNumber r => l % r,
                        OperatorBinaryTag.Lt when left is ObjectNumber l && right is ObjectNumber r => l < r,
                        OperatorBinaryTag.LtEq when left is ObjectNumber l && right is ObjectNumber r => l <= r,
                        OperatorBinaryTag.Gt when left is ObjectNumber l && right is ObjectNumber r => l > r,
                        OperatorBinaryTag.GtEq when left is ObjectNumber l && right is ObjectNumber r => l >= r,
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
                        OperatorUnaryTag.Not => !left.IsTruthy(),
                        OperatorUnaryTag.Neg when left is ObjectNumber n => -n,
                        _ =>
                            throw new RuntimeErrorException(
                                op.Position,
                                $"Operator {op.Op} cannot be applied to values <{left}>"),
                    };
                    break;

                case Refinement rfnt:
                    result = Evaluate(rfnt.Target).Get(rfnt.Name.Name) ??
                                throw new RuntimeErrorException(
                                    rfnt.Name.Position,
                                    $"Object doesn't have property <{rfnt.Name.Name}>.");
                    break;

                case Var var:
                    result = Evaluate(var.InitialValue);
                    if (!Environment.Create(var.Name.Name, result))
                        throw new RuntimeErrorException(
                            var.Name.Position,
                            $"Identifier <{var.Name.Name}> is already bound");
                    break;

                case While @while:
                    while (Evaluate(@while.Guard).IsTruthy())
                        Evaluate(@while.Body);
                    result = new ObjectNull();
                    break;

                case Write write:
                    foreach (var e in write.Arguments)
                        Console.Write(Evaluate(e));
                    result = new ObjectNull();
                    break;

                default:
                    throw new NotImplementedException(
                        $"Unimplemented {expression.GetType()}");
            }
            return result;
        }
    }
}