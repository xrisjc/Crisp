using Crisp.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Eval
{
    static class Evaluator
    {
        public static IObj Evaluate(this IExpression expression, Environment environment)
        {
            switch (expression)
            {
                case Assignment<Indexing> e
                when e.Target.Indexable.Evaluate(environment) is IIndexSet indexSet:
                    return indexSet.IndexSet(
                        e.Target.Index.Evaluate(environment),
                        e.Value.Evaluate(environment));

                case Assignment<Indexing> e:
                    throw new RuntimeErrorException("Non-indexable object indexed.");

                case Assignment<Identifier> e:
                    return environment.Set(e.Target.Name, e.Value.Evaluate(environment));

                case Assignment<Member> e:
                    {
                        var member = e.Target.Expression.Evaluate(environment) as IMemberSet;
                        if (member == null)
                        {
                            throw new RuntimeErrorException("Object doesn't have members.");
                        }
                        var name = e.Target.MemberIdentifier.Name;
                        var value1 = e.Value.Evaluate(environment);
                        var (value2, status) = member.MemberSet(name, value1);
                        if (status == SetStatus.NotFound)
                        {
                            throw new RuntimeErrorException($"Member {name} not found.");
                        }
                        return value2;
                    }

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
                    if (call.FunctionExpression.Evaluate(environment) is IFn function)
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

                case Member ml
                when ml.Expression.Evaluate(environment) is IMemberGet mg:
                    {
                        var (value, status) = mg.MemberGet(ml.MemberIdentifier.Name);
                        if (status == GetStatus.Got)
                        {
                            return value;
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                $"cannot get member {ml.MemberIdentifier.Name}");
                        }
                    }

                case Member ml:
                    throw new RuntimeErrorException(
                        $"cannot get member {ml.MemberIdentifier.Name}");

                case Function fn:
                    return new ObjFnNative(fn, environment);

                case Let let:
                    return environment.Create(let.Identifier.Name, let.Value.Evaluate(environment));

                case Identifier identifier:
                    return environment.Get(identifier.Name);

                case Indexing indexing 
                when indexing.Indexable.Evaluate(environment) is IIndexGet indexable:
                    return indexable.IndexGet(indexing.Index.Evaluate(environment));

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
                when and.Op == OperatorInfix.And:
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
                when or.Op == OperatorInfix.Or:
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
                                    "Right hand side of 'or' must be Boolean.");
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

                case OperatorUnary opUn:
                    return Evaluate(
                        opUn.Op,
                        opUn.Expression.Evaluate(environment));

                case Record rec when rec.Members.Count == 0:
                    return new ObjRecord();

                case Record rec:
                    return new ObjRecord(rec.Members.Select(x => x.Name).ToList());

                case RecordConstructor ctor
                when ctor.Record.Evaluate(environment) is ObjRecord rec:
                    {
                        var members = new Dictionary<string, IObj>();
                        foreach (var (id, expr) in ctor.Initializers)
                        {
                            var value = expr.Evaluate(environment);
                            members[id.Name] = value;
                        }
                        return rec.Construct(members);
                    }

                case RecordConstructor ctor:
                    throw new RuntimeErrorException(
                        $"Record construction requires a record object.");

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

        static IObj Bool(bool b) => b ? ObjBool.True : ObjBool.False;
        static IObj Int(int i) => new ObjInt(i);
        static IObj Float(double d) => new ObjFloat(d);
        static IObj Str(string s) => new ObjStr(s);

        public static IObj Evaluate(OperatorPrefix op, IObj obj)
        {
            switch (op)
            {
                case OperatorPrefix.Neg when (obj is ObjInt   o): return Int  (-o.Value);
                case OperatorPrefix.Neg when (obj is ObjFloat o): return Float(-o.Value);

                case OperatorPrefix.Not when (obj is ObjBool  o): return Bool (!o.Value);

                default:
                    throw new RuntimeErrorException($"Operator {op} cannot be applied to value <{obj}>");
            }
        }

        public static IObj Evaluate(OperatorInfix op, IObj left, IObj right)
        {
            switch (op)
            {
                case OperatorInfix.Add  when (left is ObjStr   l) && (right is ObjStr   r) : return Str  (l.Value +  r.Value);
                case OperatorInfix.Add  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value +  r.Value);
                case OperatorInfix.Add  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value +  r.Value);
                case OperatorInfix.Add  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value +  r.Value);
                case OperatorInfix.Add  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value +  r.Value);

                case OperatorInfix.Sub  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value -  r.Value);
                case OperatorInfix.Sub  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value -  r.Value);
                case OperatorInfix.Sub  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value -  r.Value);
                case OperatorInfix.Sub  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value -  r.Value);

                case OperatorInfix.Mul  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value *  r.Value);
                case OperatorInfix.Mul  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value *  r.Value);
                case OperatorInfix.Mul  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value *  r.Value);
                case OperatorInfix.Mul  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value *  r.Value);

                case OperatorInfix.Div  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value /  r.Value);
                case OperatorInfix.Div  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value /  r.Value);
                case OperatorInfix.Div  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value /  r.Value);
                case OperatorInfix.Div  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value /  r.Value);

                case OperatorInfix.Mod  when (left is ObjInt   l) && (right is ObjInt   r) : return Int  (l.Value %  r.Value);
                case OperatorInfix.Mod  when (left is ObjFloat l) && (right is ObjFloat r) : return Float(l.Value %  r.Value);
                case OperatorInfix.Mod  when (left is ObjInt   l) && (right is ObjFloat r) : return Float(l.Value %  r.Value);
                case OperatorInfix.Mod  when (left is ObjFloat l) && (right is ObjInt   r) : return Float(l.Value %  r.Value);

                case OperatorInfix.Lt   when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value <  r.Value);
                case OperatorInfix.Lt   when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value <  r.Value);
                case OperatorInfix.Lt   when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value <  r.Value);
                case OperatorInfix.Lt   when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value <  r.Value);

                case OperatorInfix.LtEq when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value <= r.Value);
                case OperatorInfix.LtEq when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value <= r.Value);
                case OperatorInfix.LtEq when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value <= r.Value);
                case OperatorInfix.LtEq when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value <= r.Value);

                case OperatorInfix.Gt   when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value >  r.Value);
                case OperatorInfix.Gt   when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value >  r.Value);
                case OperatorInfix.Gt   when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value >  r.Value);
                case OperatorInfix.Gt   when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value >  r.Value);

                case OperatorInfix.GtEq when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value >= r.Value);
                case OperatorInfix.GtEq when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value >= r.Value);
                case OperatorInfix.GtEq when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value >= r.Value);
                case OperatorInfix.GtEq when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value >= r.Value);

                case OperatorInfix.And  when (left is ObjBool  l) && (right is ObjBool  r) : return Bool (l.Value && r.Value);

                case OperatorInfix.Or   when (left is ObjBool  l) && (right is ObjBool  r) : return Bool (l.Value || r.Value);

                case OperatorInfix.Eq   when (left is ObjStr   l) && (right is ObjStr   r) : return Bool (l.Value == r.Value);
                case OperatorInfix.Eq   when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value == r.Value);
                case OperatorInfix.Eq   when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value == r.Value);
                case OperatorInfix.Eq   when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value == r.Value);
                case OperatorInfix.Eq   when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value == r.Value);

                case OperatorInfix.Neq  when (left is ObjStr   l) && (right is ObjStr   r) : return Bool (l.Value != r.Value);
                case OperatorInfix.Neq  when (left is ObjInt   l) && (right is ObjInt   r) : return Bool (l.Value != r.Value);
                case OperatorInfix.Neq  when (left is ObjFloat l) && (right is ObjFloat r) : return Bool (l.Value != r.Value);
                case OperatorInfix.Neq  when (left is ObjInt   l) && (right is ObjFloat r) : return Bool (l.Value != r.Value);
                case OperatorInfix.Neq  when (left is ObjFloat l) && (right is ObjInt   r) : return Bool (l.Value != r.Value);

                case OperatorInfix.Neq: return Bool(!ReferenceEquals(left, right));
                case OperatorInfix.Eq:  return Bool( ReferenceEquals(left, right));

                default:
                    throw new RuntimeErrorException(
                        $"Operator {op} cannot be applied to values " +
                        $"<{left}> and <{right}>");
            }
        }
    }
}
