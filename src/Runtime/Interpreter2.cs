using Crisp.Ast;
using Crisp.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.src.Runtime
{
    class Interpreter2
    {
        record Fn(IExpression Body, ImmutableList<Cell> Closure);

        public static void Evaluate(IExpression expression)
        {


            static bool Eq(Cell left, Cell right)
                => (left.Value is Null && right.Value is Null)
                || left.Value.Equals(right.Value);
            static bool Truthy(Cell cell)
                => cell.Value switch { bool x => x, Null _ => false, _ => true };

            IExpression expr = expression;
            ImmutableList<Cell> env = ImmutableList<Cell>.Empty;
            IContinuation cont = new ContinuationDone();
            Cell value = new Cell();

        ValueOf:
            switch (expr)
            {
                case AssignmentIdentifier ai:
                    cont = new ContinuationAssignment(env, cont, ai.Target.Depth);
                    expr = ai.Value;
                    goto ValueOf;

                case Conditional c:
                    cont = new ContinuationConditional(c.Consequence, c.Alternative, env, cont);
                    expr = c.Condition;
                    goto ValueOf;

                case ExpressionPair ep:
                    cont = new ContinuationEval(ep.Tail, env, cont);
                    expr = ep.Head;
                    goto ValueOf;

                case Ast.Function af:
                    value = new Cell(new Fn(af.Body, env));
                    goto ApplyCont;

                case FunctionCall fc:
                    cont = new ContinuationFnArg(fc.Argument, env, cont);
                    expr = fc.Target;
                    goto ApplyCont;

                case LiteralBool lb:
                    value = new Cell(lb.Value);
                    goto ApplyCont;

                case LiteralNull:
                    value = new Cell();
                    goto ApplyCont;

                case LiteralNumber ln:
                    value = new Cell(ln.Value);
                    goto ApplyCont;

                case LiteralString ls:
                    value = new Cell(ls.Value);
                    goto ApplyCont;
            }

        ApplyCont:
            switch (cont)
            {
                case ContinuationAssignment ca:
                    ca.Environment[ca.Depth].Value = value.Value;
                    cont = ca.Continuation;
                    goto ApplyCont;

                case ContinuationDone:
                    break;

                case ContinuationConditional cc:
                    expr = Truthy(value) ? cc.Consequence : cc.Alternative;
                    env = cc.Environment;
                    cont = cc.Continuation;
                    goto ValueOf;

                case ContinuationEval ce:
                    expr = ce.Expression;
                    env = ce.Environment;
                    cont = ce.Continuation;
                    goto ValueOf;

                case ContinuationFnArg cfa:
                    if (value.Value is Fn fn)
                    {
                        expr = fn.Body;
                        expr = cfa.Environment.Add(value);ldkbv9

                        Push(
                            stack,
                            fc.Argument,
                            env,
                            arg => rf(arg, cont));
                    }
                    else
                        throw new RuntimeErrorException(
                            fc.Position,
                            $"Cannot call non-callable object <{x}>.");

            }
        }
    }
}
