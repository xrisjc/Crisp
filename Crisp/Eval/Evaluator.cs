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

                case Literal<int> literal:
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
                when and.Op == Operator.And:
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
                when or.Op == Operator.Or:
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
            IObj Bool(bool b) => b ? ObjBool.True : ObjBool.False;
            IObj Int(int i) => new ObjInt(i);
            IObj Float(double d) => new ObjFloat(d);
            IObj Str(string s) => new ObjStr(s);

            switch (op)
            {
                case Operator.Add  when (left is ObjStr   l) && (right is ObjStr   r) : return Str  (l.Value +  r.Value);
                case Operator.Add  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value +  r.Value);
                case Operator.Add  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value +  r.Value);
                case Operator.Add  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value +  r.Value);
                case Operator.Add  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value +  r.Value);

                case Operator.Sub  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value -  r.Value);
                case Operator.Sub  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value -  r.Value);
                case Operator.Sub  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value -  r.Value);
                case Operator.Sub  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value -  r.Value);

                case Operator.Mul  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value *  r.Value);
                case Operator.Mul  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value *  r.Value);
                case Operator.Mul  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value *  r.Value);
                case Operator.Mul  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value *  r.Value);

                case Operator.Div  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value /  r.Value);
                case Operator.Div  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value /  r.Value);
                case Operator.Div  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value /  r.Value);
                case Operator.Div  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value /  r.Value);

                case Operator.Mod  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value %  r.Value);
                case Operator.Mod  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value %  r.Value);
                case Operator.Mod  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value %  r.Value);
                case Operator.Mod  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value %  r.Value);

                case Operator.Lt   when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value <  r.Value);
                case Operator.Lt   when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value <  r.Value);
                case Operator.Lt   when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value <  r.Value);
                case Operator.Lt   when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value <  r.Value);

                case Operator.LtEq when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value <= r.Value);
                case Operator.LtEq when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value <= r.Value);
                case Operator.LtEq when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value <= r.Value);
                case Operator.LtEq when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value <= r.Value);

                case Operator.Gt   when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value >  r.Value);
                case Operator.Gt   when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value >  r.Value);
                case Operator.Gt   when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value >  r.Value);
                case Operator.Gt   when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value >  r.Value);

                case Operator.GtEq when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value >= r.Value);
                case Operator.GtEq when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value >= r.Value);
                case Operator.GtEq when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value >= r.Value);
                case Operator.GtEq when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value >= r.Value);

                case Operator.And  when (left is ObjBool  l) && (right is ObjBool  r) : return Bool (l.Value && r.Value);

                case Operator.Or   when (left is ObjBool  l) && (right is ObjBool  r) : return Bool (l.Value || r.Value);

                case Operator.Eq   when (left is ObjStr   l) && (right is ObjStr   r) : return Bool (l.Value == r.Value);
                case Operator.Eq   when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value == r.Value);
                case Operator.Eq   when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value == r.Value);
                case Operator.Eq   when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value == r.Value);
                case Operator.Eq   when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value == r.Value);

                case Operator.Neq  when (left is ObjStr   l) && (right is ObjStr   r) : return Bool (l.Value != r.Value);
                case Operator.Neq  when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value != r.Value);
                case Operator.Neq  when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value != r.Value);
                case Operator.Neq  when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value != r.Value);
                case Operator.Neq  when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value != r.Value);

                case Operator.Neq: return Bool(!ReferenceEquals(left, right));
                case Operator.Eq:  return Bool( ReferenceEquals(left, right));

                default:
                    throw new RuntimeErrorException(
                        $"Operator {op} cannot be applied to values " +
                        $"<{left.Print()}> and <{right.Print()}>");
            }
        }
    }
}
