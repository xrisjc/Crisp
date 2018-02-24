﻿using Crisp.Ast;
using System;
using System.Linq;

namespace Crisp.Eval
{
    static class Evaluator
    {
        public static IObj Evaluate(this IExpression expression, Environment environment)
        {
            switch (expression)
            {
                case AssignmentIndex e
                when e.Index.Indexable.Evaluate(environment) is IIndexSet indexSet:
                    return indexSet.Set(
                        e.Index.Index.Evaluate(environment),
                        e.Value.Evaluate(environment));

                case AssignmentIndex e:
                    throw new RuntimeErrorException("Non-indexable object indexed.");

                case AssignmentVariable e:
                    return environment.Set(e.Identifier.Name, e.Value.Evaluate(environment));

                case Block block:
                    {
                        var localEnvironment = new Environment(environment);
                        IObj result = ObjNull.Instance;
                        foreach (var expr in block.Body)
                        {
                            result = expr.Evaluate(localEnvironment);
                        }
                        return result;
                    }

                case Branch branch:
                    {
                        var objResult = branch.Condition.Evaluate(environment);
                        if (objResult is ObjBool boolResult)
                        {
                            var expr = boolResult.Value ? branch.Consequence : branch.Alternative;
                            return expr.Evaluate(environment);
                        }

                        throw new RuntimeErrorException(
                            "an if condition must be a bool value");
                    }

                case Call call:
                    if (call.FunctionExpression.Evaluate(environment) is IObjFn function)
                    {
                        var arguments = call.ArgumentExpressions
                                            .Select(arg => arg.Evaluate(environment))
                                            .ToList();
                        return function.Call(arguments);
                    }
                    else
                    {
                        throw new RuntimeErrorException(
                            "function call attempted on non function value");
                    }

                case NamedFunction fn:
                    return environment.Create(fn.Name.Name, new ObjFnNative(fn, environment));

                case Function fn:
                    return new ObjFnNative(fn, environment);

                case Let let:
                    return environment.Create(let.Identifier, let.Value.Evaluate(environment));

                case Identifier identifier:
                    return environment.Get(identifier);

                case Indexing indexing 
                when indexing.Indexable.Evaluate(environment) is IIndexGet indexable:
                    return indexable.Get(indexing.Index.Evaluate(environment));

                case Indexing indexing:
                    throw new RuntimeErrorException("Non-indexable object indexed.");

                case Literal<bool> literal:
                    return literal.Value ? ObjBool.True : ObjBool.False;

                case Literal<long> literal:
                    return new ObjInt(literal.Value);

                case Literal<double> literal:
                    return new ObjFloat(literal.Value);

                case Literal<string> literal:
                    return new ObjStr(literal.Value);

                case LiteralNull literal:
                    return ObjNull.Instance;

                case Map map:
                    return new ObjMap(map, environment);

                case OperatorBinary and
                when and.Op == Operator.LogicalAnd:
                    {
                        var objLeft = and.Left.Evaluate(environment);
                        if (objLeft is ObjBool boolLeft)
                        {
                            if (boolLeft.Value == false)
                            {
                                return boolLeft;
                            }

                            var objRight = and.Right.Evaluate(environment);
                            if (objRight is ObjBool boolRight)
                            {
                                return boolRight;
                            }
                            else
                            {
                                throw new RuntimeErrorException(
                                    "Right hand side of 'and' must be Boolean.");
                            }
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                "Left hand side of 'and' must be Boolean.");
                        }
                    }

                case OperatorBinary or
                when or.Op == Operator.LogicalOr:
                    {
                        var objLeft = or.Left.Evaluate(environment);
                        if (objLeft is ObjBool boolLeft)
                        {
                            if (boolLeft.Value)
                            {
                                return boolLeft;
                            }

                            var objRight = or.Right.Evaluate(environment);
                            if (objRight is ObjBool boolRight)
                            {
                                return boolRight;
                            }
                            else
                            {
                                throw new RuntimeErrorException(
                                    "Right hand side of 'and' must be Boolean.");
                            }
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                "Left hand side of 'or' must be Boolean.");
                        }
                    }

                case OperatorBinary opBi:
                    return Evaluate(
                        opBi.Op,
                        opBi.Left.Evaluate(environment),
                        opBi.Right.Evaluate(environment));

                case While @while:
                    while (true)
                    {
                        var predicate = @while.Guard.Evaluate(environment);
                        if (predicate is ObjBool boolPredicate)
                        {
                            if (boolPredicate.Value == false)
                            {
                                return ObjNull.Instance;
                            }
                        }
                        else
                        {
                            throw new RuntimeErrorException("a while guard must be a bool value");
                        }
                        @while.Body.Evaluate(environment);
                    }

                default:
                    throw new RuntimeErrorException(
                        $"Unsupported AST node {expression.GetType()}.");
            }
        }

        public static IObj Evaluate(Operator op, IObj left, IObj right)
        {
            switch (op)
            {
                case Operator.EqualTo:   return left.Equals(right) ? ObjBool.True : ObjBool.False;
                case Operator.InequalTo: return left.Equals(right) ? ObjBool.False : ObjBool.True;
            }

            if (left is IComparable<IObj> lc)
            {
                switch (op)
                {
                    case Operator.GreaterThan:
                        return lc.CompareTo(right) > 0 ? ObjBool.True : ObjBool.False;

                    case Operator.GreaterThanOrEqualTo:
                        return lc.CompareTo(right) >= 0 ? ObjBool.True : ObjBool.False;

                    case Operator.LessThan:
                        return lc.CompareTo(right) < 0 ? ObjBool.True : ObjBool.False;

                    case Operator.LessThanOrEqualTo:
                        return lc.CompareTo(right) <= 0 ? ObjBool.True : ObjBool.False;
                }
            }

            if (left  is INumeric ln && 
                right is INumeric rn)
            {
                switch (op)
                {
                    case Operator.Add:      return ln.AddTo(rn);
                    case Operator.Divide:   return ln.DivideBy(rn);
                    case Operator.Modulo:   return ln.ModuloOf(rn);
                    case Operator.Multiply: return ln.MultiplyBy(rn);
                    case Operator.Subtract: return ln.SubtractBy(rn);
                }
            }

            if (left  is ObjStr ls &&
                right is ObjStr rs)
            {
                switch (op)
                {
                    case Operator.Add: return new ObjStr(ls.Value + rs.Value);
                }
            }

            throw new RuntimeErrorException(
                $"Operator {op} cannot be applied to values {left.Print()} and " +
                $"{right.Print()}");
        }
    }
}
