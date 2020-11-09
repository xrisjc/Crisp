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
            static void Push<TExpr, TEnv, TCont>(
                Stack<(TExpr, TEnv, TCont)> stack,
                TExpr expr,
                TEnv env,
                TCont cont)
            {
                stack.Push((expr, env, cont));
            }

            static Action<Cell, TCont> MakeFn<TExpr, TCont>(
                Stack<(TExpr, ImmutableList<Cell>, TCont)> stack,
                TExpr expr,
                ImmutableList<Cell> env)
            {
                return (arg, cont) => Push(stack, expr, env.Add(arg), cont);
            }

            static Action<TCont> MakeProc<TExpr, TCont>(
                Stack<(TExpr, ImmutableList<Cell>, TCont)> stack,
                IExpression expr,
                ImmutableList<Cell> env)
            {
                return cont => Push(stack, expr, env, cont);
            }

            static ImmutableList<Cell> LetRecFnExtend(
                IExpression expr,
                ImmutableList<Cell> env)
            {
                var cell = new Cell();
                var extendedEnv = env.Add(cell);
                cell.Value = MakeFn(expr, extendedEnv);
                return extendedEnv;
            }

            static ImmutableList<Cell> LetRecProcExtend(
                IExpression expr,
                ImmutableList<Cell> env)
            {
                var cell = new Cell();
                var extendedEnv = env.Add(cell);
                cell.Value = MakeProc(env, extendedEnv);
                return extendedEnv;
            }

            var stack = new Stack<(IExpression, ImmutableList<Cell>, Action<Cell>)>();

            Push(stack, program.Body, ImmutableList<Cell>.Empty, _ => { });

            while (stack.Count > 0)
            {
                var (expr, env, cont) = stack.Pop();

                //Console.WriteLine($"{expr}");

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
                                    x.IsTruthy() switch
                                    {
                                        true => c.Consequence,
                                        false => c.Alternative,
                                    },
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
                                x.Value switch
                                {
                                    Function rf =>
                                        Push(
                                            stack,
                                            fc.Argument,
                                            env,
                                            arg => rf(arg, cont)),
                                    _ =>
                                        throw new RuntimeErrorException(
                                            fc.Position,
                                            $"Cannot call non-callable object <{target}>."),
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

                    case LetRec lr when lr.Callable is Ast.Function lrf:
                        Push(
                            stack,
                            lr.Body,
                            LetRecExtend(lrf.Parameter.Name, lrf.Body, env),
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

                    case Ast.Procedure p:
                        cont(new Cell(CreateProcedure(p.Body, environment))),
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
