using System;
using System.Collections.Immutable;
using Crisp.Ast;

namespace Crisp.Runtime
{
    delegate Thunk? Thunk();

    delegate Thunk? Continuation(Cell result);

    delegate Thunk Function(Cell argument, Continuation continuation);

    delegate Thunk Procedure(Continuation continuation);

    record Null
    {
        public override string ToString() => "null";
    }

    static class InterpreterCps
    {
        public static Cell Evaluate(
            IExpression expression,
            ImmutableList<Cell> environment)
        {
            var value = new Cell();
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

        static Thunk Evaluate(
            IExpression expression,
            ImmutableList<Cell> environment,
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
                            environment[ai.Target.Depth].Value = result.Value;
                            return () => continuation(result);
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
                                conditionResult.IsTruthy() switch
                                {
                                    true => c.Consequence,
                                    false => c.Alternative,
                                },
                                environment,
                                continuation)),

                ExpressionPair ep =>
                    () => Evaluate(
                        ep.Head,
                        environment,
                        _ => () => Evaluate(ep.Tail, environment, continuation)),

                Ast.Function af =>
                    () => continuation(new Cell(CreateFunction(af.Parameter.Name, af.Body, environment))),

                FunctionCall fc =>
                    () => Evaluate(
                        fc.Target,
                        environment,
                        target =>
                            target.Value switch
                            {
                                Function rf =>
                                    () => Evaluate(
                                        fc.Argument,
                                        environment,
                                        argument => () => rf(argument, continuation)),
                                _ =>
                                    throw new RuntimeErrorException(
                                        fc.Position,
                                        $"Cannot call non-callable object <{target}>."),
                            }),

                Identifier id =>
                    () => continuation(environment[id.Depth]),

                Let l =>
                    () => Evaluate(
                        l.InitialValue,
                        environment,
                        result =>
                            () => Evaluate(
                                l.Body,
                                environment.Add(result),
                                continuation)),

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
                    () => continuation(new Cell(lb.Value)),

                LiteralNull =>
                    () => continuation(new Cell()),

                LiteralNumber ln =>
                    () => continuation(new Cell(ln.Value)),

                LiteralString ls =>
                    () => continuation(new Cell(ls.Value)),

                OperatorBinary op when op.Tag == OperatorBinaryTag.And =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left =>
                            left.IsTruthy() switch
                            {
                                true =>
                                    () => Evaluate(
                                        op.Right,
                                        environment,
                                        right => continuation(new Cell(right.IsTruthy()))),
                                false =>
                                    () => continuation(new Cell(false)),
                            }),

                OperatorBinary op when op.Tag == OperatorBinaryTag.Or =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left =>
                            left.IsTruthy() switch
                            {
                                true =>
                                    () => continuation(new Cell(true)),
                                false =>
                                    () => Evaluate(
                                        op.Right,
                                        environment,
                                        right => continuation(new Cell(right.IsTruthy()))),
                            }),

                OperatorBinary op when op.Tag == OperatorBinaryTag.Eq =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left => Evaluate(
                            op.Right,
                            environment,
                            right => continuation(new Cell(left.Equals(right))))),

                OperatorBinary op when op.Tag == OperatorBinaryTag.Neq =>
                    () => Evaluate(
                        op.Left,
                        environment,
                        left => Evaluate(
                            op.Right,
                            environment,
                            right => continuation(new Cell(!left.Equals(right))))),

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
                                        new Cell((op.Tag, left.Value, right.Value) switch
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
                                        })))),

                OperatorUnary op when op.Op == OperatorUnaryTag.Not =>
                    () => Evaluate(
                        op.Expression,
                        environment,
                        result => continuation(new Cell(!result.IsTruthy()))),

                OperatorUnary op =>
                    () => Evaluate(
                        op.Expression,
                        environment,
                        result =>
                            continuation(
                                new Cell((op.Op, result.Value) switch
                                {
                                    (OperatorUnaryTag.Neg, double n) => -n,
                                    _ => throw new RuntimeErrorException(
                                            op.Position,
                                            $"Operator {op.Op} cannot be applied to values `{result}`"),
                                }))),

                Ast.Procedure p =>
                    () => continuation(new Cell(CreateProcedure(p.Body, environment))),

                ProcedureCall pc =>
                    () => Evaluate(
                        pc.Target,
                        environment,
                        target =>
                            target.Value switch
                            {
                                Procedure rp =>
                                    () => rp(continuation),
                                object invalidTarget =>
                                    throw new RuntimeErrorException(
                                        pc.Position,
                                        $"Cannot call non-callable object <{invalidTarget}>."),
                            }),

                Program p =>
                    () => Evaluate(p.Body, environment, continuation),

                While w =>
                    () => Evaluate(
                        w.Guard,
                        environment,
                        guardResult =>
                            guardResult.IsTruthy() switch
                            {
                                true =>
                                    () => Evaluate(
                                        w.Body,
                                        environment,
                                        _ => () => Evaluate(w, environment, continuation)),
                                false =>
                                    () => continuation(new Cell()),
                            }),

                Write w =>
                    () => Evaluate(
                        w.Value,
                        environment,
                        value =>
                        {
                            Console.Write(value.Value);
                            return continuation(new Cell());
                        }),

                _ =>
                    throw new NotImplementedException(),
            };
        }

        static Function CreateFunction(
            string parameter,
            IExpression body,
            ImmutableList<Cell> environment)
        {
            return (Cell argument, Continuation continuation) =>
                () => Evaluate(body, environment.Add(argument), continuation);
        }

        static Procedure CreateProcedure(
            IExpression body,
            ImmutableList<Cell> environment)
        {
            return (Continuation continuation) =>
                () => Evaluate(body, environment, continuation);
        }

        static ImmutableList<Cell> LetRecExtend(
            string name,
            string parameter,
            IExpression body,
            ImmutableList<Cell> environment)
        {
            var cell = new Cell();
            var extendedEnvironment = environment.Add(cell);
            cell.Value = CreateFunction(parameter, body, extendedEnvironment);
            return extendedEnvironment;
        }

        static ImmutableList<Cell> LetRecExtend(
            string name,
            IExpression body,
            ImmutableList<Cell> environment)
        {
            var cell = new Cell();
            var extendedEnvironment = environment.Add(cell);
            cell.Value = CreateProcedure(body, extendedEnvironment);
            return extendedEnvironment;
        }
    }
}
