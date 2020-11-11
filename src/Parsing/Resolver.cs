using Crisp.Ast;
using System;
using System.Collections.Immutable;

namespace Crisp.Parsing
{
    static class Resolver
    {
        public static void Resolve(
            IExpression expression,
            ImmutableList<string> symbols)
        {
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    Resolve(ai.Value, symbols);
                    Resolve(ai.Target, symbols);
                    break;

                case Conditional c:
                    Resolve(c.Condition, symbols);
                    Resolve(c.Consequence, symbols);
                    Resolve(c.Alternative, symbols);
                    break;

                case ExpressionPair ep:
                    Resolve(ep.Head, symbols);
                    Resolve(ep.Tail, symbols);
                    break;
                
                case Function f:
                    Resolve(f.Body, symbols.Add(f.Parameter.Name));
                    break;

                case FunctionCall fc:
                    Resolve(fc.Target, symbols);
                    Resolve(fc.Argument, symbols);
                    break;

                case Identifier i:
                    i.Depth = symbols.FindLastIndex(sym => sym == i.Name);
                    if (i.Depth < 0)
                        throw new SyntaxErrorException(
                            $"undefined symbol `{i.Name}`",
                            i.Position);
                    break;

                case Let l:
                    Resolve(l.InitialValue, symbols);
                    Resolve(l.Body, symbols.Add(l.Name.Name));
                    break;

                case LetRec lr:
                    Resolve(lr.Callable, lr.Body, symbols.Add(lr.Name));
                    break;
                
                case LiteralBool:
                case LiteralNull:
                case LiteralNumber:
                case LiteralString:
                    break;

                case OperatorBinary ob:
                    Resolve(ob.Left, symbols);
                    Resolve(ob.Right, symbols);
                    break;

                case OperatorUnary ou:
                    Resolve(ou.Expression, symbols);
                    break;

                case Program p:
                    Resolve(p.Body, symbols);
                    break;

                case While w:
                    Resolve(w.Guard, symbols);
                    Resolve(w.Body, symbols);
                    break;

                case Write w:
                    Resolve(w.Value, symbols);
                    break;

                case IExpression e:
                    throw new NotImplementedException(
                        $"resolve not implemented for {e.GetType()}");
            }
        }

        public static void Resolve(
            IExpression expression1,
            IExpression expression2,
            ImmutableList<string> symbols)
        {
            Resolve(expression1, symbols);
            Resolve(expression2, symbols);
        }
    }
}
