using System;
using Crisp.Ast;

namespace Crisp.Runtime
{
    delegate Thunk? Thunk();

    delegate Thunk? Continuation(object result);

    delegate Thunk Function(object[] arguments, Continuation continuation);

    record Null
    {
        public override string ToString() => "null";
    }

    static class InterpreterCps
    {
        public static object Evaluate(IExpression expression, Environment environment)
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

        private static Thunk Evaluate(
            IExpression expression,
            Environment environment,
            Continuation continuation)
        {
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    return () => Evaluate(
                        ai.Value,
                        environment,
                        result =>
                        {
                            if (!environment.Set(ai.Target.Name, result))
                                throw new RuntimeErrorException(
                                    ai.Target.Position,
                                    $"Cannot assign, <{ai.Target.Name}> is unbound");
                            return () => continuation(result);
                        });

                case Block b:
                    return () => Evaluate(
                        b.Body,
                        new Environment(environment),
                        continuation);

                case Call c:
                    return () => Evaluate(
                        c.Target,
                        environment,
                        result =>
                        {
                            if (result is Function rf)
                            {
                                var arguments = new object[c.Arguments.Count];
                                for (var i = 0; i < arguments.Length; i++)
                                    arguments[i] = Evaluate(c.Arguments[i], environment);
                                return () => rf(arguments, continuation);
                            }
                            else
                            {
                                throw new RuntimeErrorException(
                                    c.Position,
                                    $"Cannot call non-callable object <{result}>.");
                            }
                        });

                case Conditional c:
                    return () => Evaluate(
                        c.Condition,
                        environment,
                        conditionResult =>
                            () => Evaluate(
                                IsTruthy(conditionResult)
                                    ? c.Consequence
                                    : c.Alternative,
                                environment,
                                continuation));

                case ExpressionPair ep:
                    return () => Evaluate(
                        ep.Head,
                        environment,
                        _ => () => Evaluate(ep.Tail, environment, continuation));

                case Ast.Function af:
                    {
                        var closure = environment;

                        Function rf = (object[] arguments, Continuation continuation) =>
                        {
                            var environment = new Environment(closure);
                            var parameters = af.Parameters;
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
                            return () => Evaluate(af.Body, environment, continuation);
                        };

                        return () => continuation(rf);
                    };

                case Identifier id:
                    return () => continuation(
                        environment.Get(id.Name) ??
                            throw new RuntimeErrorException(
                                id.Position,
                                $"Identifier <{id.Name}> unbound"));
                
                case LiteralBool lb:
                    return () => continuation(lb.Value);

                case LiteralNull ln:
                    return () => continuation(new Null());

                case LiteralNumber ln:
                    return () => continuation(ln.Value);

                case LiteralString ls:
                    return () => continuation(ls.Value);

                case Let l:
                    return () => Evaluate(
                        l.InitialValue,
                        environment,
                        result =>
                        {
                            if (!environment.Create(l.Name.Name, result))
                                throw new RuntimeErrorException(
                                    l.Name.Position,
                                    $"Identifier <{l.Name.Name}> is already bound");
                            return () => continuation(result);
                        });

                case OperatorBinary op when op.Tag == OperatorBinaryTag.And:
                    return () => Evaluate(
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
                                return () => continuation(false);
                        });

                case OperatorBinary op when op.Tag == OperatorBinaryTag.Or:
                    return () => Evaluate(
                        op.Left,
                        environment,
                        left =>
                        {
                            if (IsTruthy(left))
                                return () => continuation(true);
                            else
                                return () => Evaluate(
                                    op.Right,
                                    environment,
                                    right => continuation(IsTruthy(right)));
                        });

                case OperatorBinary op when op.Tag == OperatorBinaryTag.Eq:
                    return () => Evaluate(
                        op.Left,
                        environment,
                        left => Evaluate(
                            op.Right,
                            environment,
                            right => () => continuation(left.Equals(right))));

                case OperatorBinary op when op.Tag == OperatorBinaryTag.Neq:
                    return () => Evaluate(
                        op.Left,
                        environment,
                        left => Evaluate(
                            op.Right,
                            environment,
                            right => () => continuation(!left.Equals(right))));

                case OperatorBinary op:
                    return () => Evaluate(
                        op.Left,
                        environment,
                        left =>
                            Evaluate(
                                op.Right,
                                environment,
                                right =>
                                    () => continuation(
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
                                        })));

                case OperatorUnary op when op.Op == OperatorUnaryTag.Not:
                    return () => Evaluate(
                        op.Expression,
                        environment,
                        result => () => continuation(!IsTruthy(result)));

                case OperatorUnary op:
                    return () => Evaluate(
                        op.Expression,
                        environment,
                        result =>
                            () => continuation(
                                (op.Op, result) switch
                                {
                                    (OperatorUnaryTag.Neg, double n) => -n,
                                    _ => throw new RuntimeErrorException(
                                            op.Position,
                                            $"Operator {op.Op} cannot be applied to values `{result}`"),
                                }));

                case While w:
                    return () =>
                    {
                        while (IsTruthy(Evaluate(w.Guard, environment)))
                            Evaluate(w.Body, environment);
                        return () => continuation(new Null());
                    };

                case Write w:
                    return () =>
                    {
                        foreach (var e in w.Arguments)
                            Console.Write(Evaluate(e, environment));
                        return () => continuation(new Null());
                    };

                default:
                    throw new NotImplementedException();
            }
        }
    }
}