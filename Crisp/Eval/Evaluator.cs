using Crisp.Ast;
using System.Linq;

namespace Crisp.Eval
{
    static class Evaluator
    {
        public static IObj Evaluate(this IExpression expression, Environment environment)
        {
            switch (expression)
            {
                case AssignmentIndex e:
                    {
                        var objectRight = e.Index.Indexable.Evaluate(environment);
                        if (objectRight is IIndexable objIndexable)
                        {
                            var objIndex = e.Index.Index.Evaluate(environment);
                            var objValue = e.Value.Evaluate(environment);
                            return objIndexable.Set(objIndex, objValue);
                        }
                        else
                        {
                            throw new RuntimeErrorException("Non-indexable object indexed.");
                        }
                    }

                case AssignmentVariable e:
                    return environment.Set(e.Identifier.Name, e.Value.Evaluate(environment));

                case Block block:
                    {
                        var localEnvironment = new Environment(environment);
                        IObj result = Obj.Null;
                        foreach (var expr in block.Body)
                        {
                            result = expr.Evaluate(localEnvironment);
                        }
                        return result;
                    }

                case Branch branch:
                    {
                        var objResult = branch.Condition.Evaluate(environment);
                        if (objResult is Obj<bool> boolResult)
                        {
                            var expr = boolResult.Value ? branch.Consequence : branch.Alternative;
                            return expr.Evaluate(environment);
                        }

                        throw new RuntimeErrorException(
                            "an if condition must be a bool value");
                    }

                case Call call:
                    {
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
                when indexing.Indexable.Evaluate(environment) is IIndexable indexable:
                    return indexable.Get(indexing.Index.Evaluate(environment));

                case Indexing indexing:
                    throw new RuntimeErrorException("Non-indexable object indexed.");

                case Literal<bool> literal:
                    return new Obj<bool>(literal);

                case Literal<double> literal:
                    return new Obj<double>(literal);

                case Literal<string> literal:
                    return new Obj<string>(literal);

                case LiteralNull literal:
                    return Obj.Null;

                case Map map:
                    return new ObjMap(map, environment);

                case OperatorBinary and when and.Op == Operator.LogicalAnd:
                    {
                        var objLeft = and.Left.Evaluate(environment);
                        if (objLeft is Obj<bool> boolLeft)
                        {
                            if (boolLeft.Value == false)
                            {
                                return boolLeft;
                            }

                            var objRight = and.Right.Evaluate(environment);
                            if (objRight is Obj<bool> boolRight)
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

                case OperatorBinary or when or.Op == Operator.LogicalOr:
                    {
                        var objLeft = or.Left.Evaluate(environment);
                        if (objLeft is Obj<bool> boolLeft)
                        {
                            if (boolLeft.Value)
                            {
                                return boolLeft;
                            }

                            var objRight = or.Right.Evaluate(environment);
                            if (objRight is Obj<bool> boolRight)
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
                        if (predicate is Obj<bool> boolPredicate)
                        {
                            if (boolPredicate.Value == false)
                            {
                                return Obj.Null;
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
                case Operator.EqualTo:   return Obj.Create( left.Equals(right));
                case Operator.InequalTo: return Obj.Create(!left.Equals(right));
            }

            if (left  is Obj<double> ld && 
                right is Obj<double> rd)
            {
                switch (op)
                {
                    case Operator.Add:                  return Obj.Create(ld.Value +  rd.Value);
                    case Operator.Divide:               return Obj.Create(ld.Value /  rd.Value);
                    case Operator.GreaterThan:          return Obj.Create(ld.Value >  rd.Value);
                    case Operator.GreaterThanOrEqualTo: return Obj.Create(ld.Value >= rd.Value);
                    case Operator.LessThan:             return Obj.Create(ld.Value <  rd.Value);
                    case Operator.LessThanOrEqualTo:    return Obj.Create(ld.Value <= rd.Value);
                    case Operator.Modulo:               return Obj.Create(ld.Value %  rd.Value);
                    case Operator.Multiply:             return Obj.Create(ld.Value *  rd.Value);
                    case Operator.Subtract:             return Obj.Create(ld.Value -  rd.Value);
                }
            }

            throw new RuntimeErrorException(
                $"Operator {op} cannot be applied to values {left.Print()} and " +
                $"{right.Print()}");
        }
    }
}
