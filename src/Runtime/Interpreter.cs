using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Crisp.Ast;

namespace Crisp.Runtime
{
    static class Interpreter
    {
        public static void Evaluate(
            IExpression expression)
        {
            var done = false;
            var rExpr = expression;
            var rEnv = ImmutableList<Cell>.Empty;
            Action<Cell> rCont = _ => { done = true; };

            static bool Eq(Cell left, Cell right)
            {
                return (left.Value is Null && right.Value is Null) || left.Value.Equals(right.Value);
            }

            static bool Truthy(Cell cell)
            {
                return cell.Value switch { bool x => x, Null _ => false, _ => true };
            }

            void Eval(IExpression expr, ImmutableList<Cell> env, Action<Cell> cont)
            {
                rExpr = expr;
                rEnv = env;
                rCont = cont;
            }

            Action<Cell, Action<Cell>> MakeFn(IExpression expr, ImmutableList<Cell> env)
            {
                return (arg, cont) => Eval(expr, env.Add(arg), cont);
            }

            ImmutableList<Cell> LetRecExtend(IExpression expr, ImmutableList<Cell> env)
            {
                var cell = new Cell();
                var extendedEnv = env.Add(cell);
                cell.Value = MakeFn(expr, extendedEnv);
                return extendedEnv;
            }


            while (!done)
            {
                // Copy references into variables for closures.
                var (expr, env, cont) = (rExpr, rEnv, rCont);

                switch (expr)
                {
                    case AssignmentIdentifier ai:
                        Eval(
                            ai.Value,
                            env,
                            x => { env[ai.Target.Depth].Value = x.Value; cont(x); });
                        break;

                    case Conditional c:
                        Eval(
                            c.Condition,
                            env,
                            x =>
                                Eval(
                                    Truthy(x) ? c.Consequence : c.Alternative,
                                    env,
                                    cont));
                        break;

                    case ExpressionPair ep:
                        Eval(ep.Head, env, _ => Eval(ep.Tail, env, cont));
                        break;

                    case Ast.Function af:
                        cont(new Cell(MakeFn(af.Body, env)));
                        break;

                    case FunctionCall fc:
                        Eval(
                            fc.Target,
                            env,
                            x =>
                            {
                                if (x.Value is Action<Cell, Action<Cell>> rf)
                                    Eval(
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
                        Eval(l.InitialValue, env, x => Eval(l.Body, env.Add(x), cont));
                        break;

                    case LetRec lr when lr.Callable is Function lrf:
                        Eval(lr.Body, LetRecExtend(lrf.Body, env), cont);
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
                        Eval(
                            op.Left,
                            env,
                            left =>
                            {
                                if (Truthy(left))
                                    Eval(
                                        op.Right,
                                        env,
                                        right => cont(new Cell(Truthy(right))));
                                else
                                    cont(new Cell(false));
                            });
                        break;

                    case OperatorBinary op when op.Tag == OperatorBinaryTag.Or:
                        Eval(
                            op.Left,
                            env,
                            left =>
                            {
                                if (Truthy(left))
                                    cont(new Cell(true));
                                else
                                    Eval(
                                        op.Right,
                                        env,
                                        right => cont(new Cell(Truthy(right))));
                            });
                        break;

                    case OperatorBinary op when op.Tag == OperatorBinaryTag.Eq:
                        Eval(
                            op.Left,
                            env,
                            left => Eval(
                                op.Right,
                                env,
                                right => cont(new Cell(Eq(left, right)))));
                        break;

                    case OperatorBinary op when op.Tag == OperatorBinaryTag.Neq:
                        Eval(
                            op.Left,
                            env,
                            left => Eval(
                                op.Right,
                                env,
                                right => cont(new Cell(!Eq(left, right)))));
                        break;

                    case OperatorBinary op:
                        Eval(
                            op.Left,
                            env,
                            left =>
                                Eval(
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
                        Eval(
                            op.Expression,
                            env,
                            x => cont(new Cell(!Truthy(x))));
                        break;

                    case OperatorUnary op:
                        Eval(
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

                    case While w:
                        Eval(
                            w.Guard,
                            env,
                            x =>
                            {
                                if (Truthy(x))
                                {
                                    Eval(w.Body, env, _ => Eval(w, env, cont));
                                }
                                else
                                    cont(new Cell());
                            });
                        break;

                    case Write w:
                        Eval(
                            w.Value,
                            env,
                            x => { Console.Write(x); cont(new Cell()); });
                        break;

                }
            }
        }
    }
}
