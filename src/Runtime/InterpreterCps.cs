using System;
using Crisp.Ast;

namespace Crisp.Runtime
{
    delegate Thunk? Thunk();

    delegate Thunk? Continuation(object result);

    delegate Thunk Function(object argument, Continuation continuation);

    delegate Thunk Procedure(Continuation continuation);

    record Null
    {
        public override string ToString() => "null";
    }

    static class InterpreterCps
    {
        public static object Evaluate(IExpression expression, IEnvironment environment)
        {
            object value = new Null();
            var thunk = Evaluate(
                expression,
                environment,
                result =>
                {
                    value = result;
                    return null;
                });
            while (thunk != null)
                thunk = thunk();
            return value;
        }

        static bool IsTruthy(object obj)
            => obj switch { bool x => x, Null _ => false, _ => true };

        static Thunk Evaluate(
            IExpression expression,
            IEnvironment environment,
            Continuation continuation)
        {
            return expression switch
            {
                AssignmentIdentifier ai =>
                    () => Evaluate(
                        ai.Value,
                        environment,
                        result =>
                        {
                            if (!environment.Set(ai.Target.Name, result))
                                throw new RuntimeErrorException(
                                    ai.Target.Position,
                                    $"Cannot assign, <{ai.Target.Name}> is unbound");
                            return continuation(result);
                        }),

                Block b =>
                    () => Evaluate(
                        b.Body,
                        environment,
                        continuation),

                Conditional c =>
                    () => Evaluate(
                        c.Condition,
                        environment,
                        conditionResult =>
                            () => Evaluate(
                                IsTruthy(conditionResult)
                                    ? c.Consequence
                                    : c.Alternative,
                                environment,
                                continuation)),

                ExpressionPair ep =>
                    () => Evaluate(
                        ep.Head,
                        environment,
                        _ => () => Evaluate(ep.Tail, environment, continuation)),

                Ast.Function af =>
                    () => continuation(CreateFunction(af.Parameter.Name, af.Body, environment)),

                FunctionCall fc =>
                    () => Evaluate(
                        fc.Target,
                        environment,
                        target =>
                        {
                            if (target is Function rf)
                            {
                                return () => Evaluate(
                                    fc.Argument,
                                    environment,
                                    argument =>
                                        () => rf(argument, continuation));
                            }
                            else
                            {
                                throw new RuntimeErrorException(
                                    fc.Position,
                                    $"Cannot call non-callable object <{target}>.");
                            }
                        }),

                Identifier id =>
                    () => continuation(
                        environment.Get(id.Name) ??
                            throw new RuntimeErrorException(
                                id.Position,
                                $"Identifier <{id.Name}> unbound")),

                Let l =>
                    () => Evaluate(
                        l.InitialValue,
                        environment,
                        result =>
                        {
                            var letEnvironment = new EnvironmentExtended(l.Name.Name, result, environment);
                            return () => Evaluate(l.Body, letEnvironment, continuation);
                        }),

                LetRec lr when lr.Callable is Ast.Function lrf =>
                    () => Evaluate(
                        lr.Body,
                        LetRecExtend(lr.Name, lrf.Parameter.Name, lrf.Body, environment),
                        continuation),

                LetRec lr when lr.Callable is Ast.Procedure lrp =>
                    () => Evaluate(
                        lr.Body,
                        LetRecExtend(lr.Name, lrp.Body, environment),
                        continuation),
                
                LiteralBool lb =>
                    () => continuation(lb.Value),

                LiteralNull =>
                    () => continuation(new Null()),

                LiteralNumber ln =>
                    () => continuation(ln.Value),

                LiteralString ls =>
                    () => continuation(ls.Value),

                OperatorBinary op when op.Tag == OperatorBinaryTag.And =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left =>
                        {
                            if (IsTruthy(left))
                                return () => Evaluate(
                                    op.Right,
                                    environment,
                                    right => continuation(IsTruthy(right)));
                            else
                                return continuation(false);
                        }),

                OperatorBinary op when op.Tag == OperatorBinaryTag.Or =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left =>
                        {
                            if (IsTruthy(left))
                                return continuation(true);
                            else
                                return () => Evaluate(
                                    op.Right,
                                    environment,
                                    right => continuation(IsTruthy(right)));
                        }),

                OperatorBinary op when op.Tag == OperatorBinaryTag.Eq =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left => Evaluate(
                            op.Right,
                            environment,
                            right => continuation(left.Equals(right)))),

                OperatorBinary op when op.Tag == OperatorBinaryTag.Neq =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left => Evaluate(
                            op.Right,
                            environment,
                            right => continuation(!left.Equals(right)))),

                OperatorBinary op =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left =>
                            Evaluate(
                                op.Right,
                                environment,
                                right =>
                                    continuation(
                                        (op.Tag, left, right) switch
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
                                        }))),

                OperatorUnary op when op.Op == OperatorUnaryTag.Not =>
                    () => Evaluate(
                        op.Expression,
                        environment,
                        result => continuation(!IsTruthy(result))),

                OperatorUnary op =>
                    () => Evaluate(
                        op.Expression,
                        environment,
                        result =>
                            continuation(
                                (op.Op, result) switch
                                {
                                    (OperatorUnaryTag.Neg, double n) => -n,
                                    _ => throw new RuntimeErrorException(
                                            op.Position,
                                            $"Operator {op.Op} cannot be applied to values `{result}`"),
                                })),

                Ast.Procedure p =>
                    () => continuation(CreateProcedure(p.Body, environment)), 

                ProcedureCall pc =>
                    () => Evaluate(
                        pc.Target,
                        environment,
                        target =>
                        {
                            if (target is Procedure rp)
                            {
                                return () => rp(continuation);
                            }
                            else
                            {
                                throw new RuntimeErrorException(
                                    pc.Position,
                                    $"Cannot call non-callable object <{target}>.");
                            }
                        }),

                Program p =>
                    () => Evaluate(p.Body, environment, continuation),

                While w =>
                    () => Evaluate(
                        w.Guard,
                        environment,
                        guardResult =>
                        {
                            if (IsTruthy(guardResult))
                                return () => Evaluate(
                                    w.Body,
                                    environment,
                                    _ => () => Evaluate(w, environment, continuation));
                            else
                                return continuation(new Null());
                        }),

                Write w =>
                    () => Evaluate(
                        w.Value,
                        environment,
                        value =>
                        {
                            Console.Write(value);
                            return continuation(new Null());
                        }),

                _ =>
                    throw new NotImplementedException(),
            };
        }

        static Function CreateFunction(string parameter, IExpression body, IEnvironment environment)
        {
            return (object argument, Continuation continuation) =>
            {
                var localEnvironment = new EnvironmentExtended(parameter, argument, environment);
                return () => Evaluate(body, localEnvironment, continuation);
            };
        }

        static Procedure CreateProcedure(IExpression body, IEnvironment environment)
        {
            return (Continuation continuation) =>
                () => Evaluate(body, environment, continuation);
        }

        static IEnvironment LetRecExtend(string name, string parameter, IExpression body, IEnvironment environment)
        {
            var extendedEnvironment = new EnvironmentExtended(name, new Null(), environment);
            extendedEnvironment.Value = CreateFunction(parameter, body, extendedEnvironment);
            return extendedEnvironment;
        }

        static IEnvironment LetRecExtend(string name, IExpression body, IEnvironment environment)
        {
            var extendedEnvironment = new EnvironmentExtended(name, new Null(), environment);
            extendedEnvironment.Value = CreateProcedure(body, extendedEnvironment);
            return extendedEnvironment;
        }
    }
}
