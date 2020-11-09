using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Crisp.Ast;

namespace Crisp.Runtime
{
    static class Interpreter
    {
        public static void Evaluate(
            Program program)
        {
            static bool Eq(Cell left, Cell right)
                => (left.Value is Null && right.Value is Null) 
                || left.Value.Equals(right.Value);
            static bool Truthy(Cell cell)
                => cell.Value switch { bool x => x, Null _ => false, _ => true };

            static void Push<TExpr, TEnv, TCont>(
                Stack<(TExpr, TEnv, TCont)> stack,
                TExpr expr,
                TEnv env,
                TCont cont)
            {
                stack.Push((expr, env, cont));
            }

            static Action<Cell, Action<Cell>> MakeFn(
                Stack<(IExpression, ImmutableList<Cell>, Action<Cell>)> stack,
                IExpression expr,
                ImmutableList<Cell> env)
            {
                return (arg, cont) => Push(stack, expr, env.Add(arg), cont);
            }

            static ImmutableList<Cell> LetRecExtend(
                Stack<(IExpression, ImmutableList<Cell>, Action<Cell>)> stack,
                IExpression expr,
                ImmutableList<Cell> env)
            {
                var cell = new Cell();
                var extendedEnv = env.Add(cell);
                cell.Value = MakeFn(stack, expr, extendedEnv);
                return extendedEnv;
            }

            var stack = new Stack<(IExpression, ImmutableList<Cell>, Action<Cell>)>();

            Push(stack, program, ImmutableList<Cell>.Empty, _ => { });

            while (stack.Count > 0)
            {
                var (expr, env, cont) = stack.Pop();

                switch (expr)
                {
                    case AssignmentIdentifier ai:
                        Push(
                            stack,
                            ai.Value,
                            env,
                            x => { env[ai.Target.Depth].Value = x.Value; cont(x); });
                        break;

                    case Block b:
                        Push(stack, b.Body, env, cont);
                        break;

                    case Conditional c:
                        Push(
                            stack,
                            c.Condition,
                            env,
                            x =>
                                Push(
                                    stack,
                                    Truthy(x) ? c.Consequence : c.Alternative,
                                    env,
                                    cont));
                        break;

                    case ExpressionPair ep:
                        Push(stack, ep.Tail, env, cont);
                        Push(stack, ep.Head, env, _ => { });
                        break;

                    case Ast.Function af:
                        cont(new Cell(MakeFn(stack, af.Body, env)));
                        break;

                    case FunctionCall fc:
                        Push(
                            stack,
                            fc.Target,
                            env,
                            x =>
                            {
                                if (x.Value is Action<Cell, Action<Cell>> rf)
                                    Push(
                                        stack,
                                        fc.Argument,
                                        env,
                                        arg => rf(arg, cont));
                                else
                                    throw new RuntimeErrorException(
                                        fc.Position,
                                        $"Cannot call non-callable object <{x}>.");
                            });
                        break;
                    
                    case Identifier id:
                        cont(env[id.Depth]);
                        break;

                    case Let l:
                        Push(
                            stack,
                            l.InitialValue,
                            env,
                            x => Push(stack, l.Body, env.Add(x), cont));
                        break;

                    case LetRec lr when lr.Callable is Function lrf:
                        Push(
                            stack,
                            lr.Body,
                            LetRecExtend(stack, lrf.Body, env),
                            cont);
                        break;

                    case LiteralBool lb:
                        cont(new Cell(lb.Value));
                        break;

                    case LiteralNull:
                        cont(new Cell());
                        break;

                    case LiteralNumber ln:
                        cont(new Cell(ln.Value));
                        break;

                    case LiteralString ls:
                        cont(new Cell(ls.Value));
                        break;

                    case OperatorBinary op when op.Tag == OperatorBinaryTag.And:
                        Push(
                            stack,
                            op.Left,
                            env,
                            left =>
                            {
                                if (Truthy(left))
                                    Push(
                                        stack,
                                        op.Right,
                                        env,
                                        right => cont(new Cell(Truthy(right))));
                                else
                                    cont(new Cell(false));
                            });
                        break;

                    case OperatorBinary op when op.Tag == OperatorBinaryTag.Or:
                        Push(
                            stack,
                            op.Left,
                            env,
                            left =>
                            {
                                if (Truthy(left))
                                    cont(new Cell(true));
                                else
                                    Push(
                                        stack,
                                        op.Right,
                                        env,
                                        right => cont(new Cell(Truthy(right))));
                            });
                        break;

                    case OperatorBinary op when op.Tag == OperatorBinaryTag.Eq:
                        Push(
                            stack,
                            op.Left,
                            env,
                            left => Push(
                                stack,
                                op.Right,
                                env,
                                right => cont(new Cell(Eq(left, right)))));
                        break;

                    case OperatorBinary op when op.Tag == OperatorBinaryTag.Neq:
                        Push(
                            stack,
                            op.Left,
                            env,
                            left => Push(
                                stack,
                                op.Right,
                                env,
                                right => cont(new Cell(!Eq(left, right)))));
                        break;

                    case OperatorBinary op:
                        Push(
                            stack,
                            op.Left,
                            env,
                            left =>
                                Push(
                                    stack,
                                    op.Right,
                                    env,
                                    right =>
                                        cont(
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
                                            }))));
                        break;

                    case OperatorUnary op when op.Op == OperatorUnaryTag.Not:
                        Push(
                            stack,
                            op.Expression,
                            env,
                            x => cont(new Cell(!Truthy(x))));
                        break;

                    case OperatorUnary op:
                        Push(
                            stack,
                            op.Expression,
                            env,
                            x =>
                                cont(
                                    new Cell((op.Op, x.Value) switch
                                    {
                                        (OperatorUnaryTag.Neg, double n) => -n,
                                        _ => throw new RuntimeErrorException(
                                                op.Position,
                                                $"Operator {op.Op} cannot be applied to values `{x}`"),
                                    })));
                        break;

                    case Program p:
                        Push(stack, p.Body, env, cont);
                        break;

                    case While w:
                        Push(
                            stack,
                            w.Guard,
                            env,
                            x =>
                            {
                                if (Truthy(x))
                                {
                                    Push(stack, w, env, cont);
                                    Push(stack, w.Body, env, _ => { });
                                }
                                else
                                    cont(new Cell());
                            });
                        break;

                    case Write w:
                        Push(
                            stack,
                            w.Value,
                            env,
                            x => { Console.Write(x); cont(new Cell()); });
                        break;

                }
            }
        }
    }
}
