using Crisp.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Eval
{
    static class Evaluator
    {
        public static dynamic Evaluate(this IExpression expression, Environment environment)
        {
            switch (expression)
            {
                case AssignmentIndexing e:
                    {
                        var target = e.Target.Indexable.Evaluate(environment);
                        if (!CheckIndexing(target))
                        {
                            throw new RuntimeErrorException("Non-indexable object indexed.");
                        }

                        var index = e.Target.Index.Evaluate(environment);
                        if (!CheckIntIndexing(target, index))
                        {
                            throw new RuntimeErrorException("Lists must be indexed by integer.");
                        }

                        var value = e.Value.Evaluate(environment);
                        target[index] = value;
                        return value;
                    }

                case AssignmentIdentifier e:
                    return environment.Set(e.Target.Name, e.Value.Evaluate(environment));

                case AssignmentMember e:
                    {
                        var member = e.Target.Expression.Evaluate(environment);
                        var name = e.Target.MemberIdentifier.Name;
                        var value = e.Value.Evaluate(environment);
                        if (!member.MemberSet(name, value))
                        {
                            throw new RuntimeErrorException($"Member {name} not found.");
                        }
                        return value;
                    }

                case Block block:
                    {
                        var localEnvironment = new Environment(environment);
                        dynamic result = null;
                        foreach (var expr in block.Body)
                        {
                            result = expr.Evaluate(localEnvironment);
                        }
                        return result;
                    }

                case Branch branch:
                    {
                        var result = branch.Condition.Evaluate(environment);
                        if (result is bool)
                        {
                            var expr = result ? branch.Consequence : branch.Alternative;
                            return expr.Evaluate(environment);
                        }

                        throw new RuntimeErrorException(
                            "an if condition must be a bool value");
                    }

                case Call call
                when call.FunctionExpression.Evaluate(environment) is ObjFn function:
                    if (function.Arity == call.Arity)
                    {
                        var arguments = call.ArgumentExpressions.Evaluate(environment);
                        return function.Call(arguments);
                    }
                    else
                    {
                        throw new RuntimeErrorException("function arity mismatch");
                    }

                case Call call:
                    throw new RuntimeErrorException(
                        "function call attempted on non function value");

                case Command command:
                    return Evaluate(
                        command.Type,
                        command.ArgumentExpressions.Evaluate(environment));

                case MemberCall call
                when call.Member.Expression.Evaluate(environment) is ObjRecordInstance record:
                    {
                        var fn = record.GetMemberFunction(call.Member.MemberIdentifier.Name);
                        if (fn.Arity != call.Arity + 1) // + 1 for "this" argument
                        {
                            throw new RuntimeErrorException("function arity mismatch");
                        }

                        var arguments = call.ArgumentExpressions.Evaluate(environment);
                        arguments.Add(record); // the "this" argument is at the end
                        return fn.Call(arguments);
                    }

                case MemberCall call:
                    throw new RuntimeErrorException("method call must be on a record instance");

                case NamedFunction fn:
                    return environment.Create(fn.Name.Name, new ObjFn(fn, environment));

                case Member ml
                when ml.Expression.Evaluate(environment) is ObjRecordInstance mg:
                    {
                        if (mg.MemberGet(ml.MemberIdentifier.Name, out var value))
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
                    return new ObjFn(fn, environment);

                case Let let:
                    return environment.Create(let.Identifier.Name, let.Value.Evaluate(environment));

                case Len len:
                    {
                        var obj = len.Expression.Evaluate(environment);
                        if (obj is Dictionary<dynamic, dynamic> || obj is List<dynamic>)
                        {
                            return obj.Count;
                        }
                        else
                        {
                            throw new RuntimeErrorException("unsupported object passed to len()");
                        }
                    }

                case Identifier identifier:
                    return environment.Get(identifier.Name);

                case Indexing indexing:
                    {
                        var target = indexing.Indexable.Evaluate(environment);
                        if (!CheckIndexing(target))
                        {
                            throw new RuntimeErrorException("Non-indexable object indexed.");
                        }

                        var index = indexing.Index.Evaluate(environment);
                        if (!CheckIntIndexing(target, index))
                        {
                            throw new RuntimeErrorException("List must be indexed by an integer.");
                        }

                        return target[index];
                    }

                case List list:
                    {
                        var items = new List<dynamic>();
                        foreach (var initializer in list.Initializers)
                        {
                            var item = initializer.Evaluate(environment);
                            items.Add(item);
                        }
                        return items;
                    }

                case LiteralBool literal:
                    return literal.Value;

                case LiteralInt literal:
                    return literal.Value;

                case LiteralDouble literal:
                    return literal.Value;

                case LiteralString literal:
                    return literal.Value;

                case LiteralNull literal:
                    return null;

                case Map map:
                    {
                        var dict = new Dictionary<dynamic, dynamic>();
                        foreach (var (index, value) in map.Initializers)
                        {
                            var objIndex = index.Evaluate(environment);
                            var objValue = value.Evaluate(environment);
                            dict[objIndex] = objValue;
                        }
                        return dict;
                    }

                case OperatorBinary and
                when and.Op == OperatorInfix.And:
                    {
                        var objLeft = and.Left.Evaluate(environment);
                        if (objLeft is bool boolLeft)
                        {
                            if (boolLeft == false)
                            {
                                return boolLeft;
                            }

                            var objRight = and.Right.Evaluate(environment);
                            if (objRight is bool boolRight)
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
                        if (objLeft is bool boolLeft)
                        {
                            if (boolLeft)
                            {
                                return boolLeft;
                            }

                            var objRight = or.Right.Evaluate(environment);
                            if (objRight is bool boolRight)
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

                case Record rec:
                    return new ObjRecord(
                        rec.Variables.Select(x => x.Name).ToList(),
                        rec.Functions.ToDictionary(
                            nf => nf.Name.Name,
                            nf => new ObjFn(nf, environment)));

                case RecordConstructor ctor
                when ctor.Record.Evaluate(environment) is ObjRecord rec:
                    {
                        var members = new Dictionary<string, dynamic>();
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
                        if (predicate is bool boolPredicate)
                        {
                            if (boolPredicate == false)
                            {
                                return null;
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

        public static List<dynamic> Evaluate(this IEnumerable<IExpression> expressions,
            Environment environment)
        {
            var results = new List<dynamic>();
            foreach (var expression in expressions)
            {
                var result = expression.Evaluate(environment);
                results.Add(result);
            }
            return results;
        }

        public static dynamic Evaluate(OperatorPrefix op, dynamic obj)
        {
            switch (op)
            {
                case OperatorPrefix.Neg when obj is int || obj is double: return -obj;
                case OperatorPrefix.Not when obj is bool                : return !obj;
                default:
                    throw new RuntimeErrorException(
                        $"Operator {op} cannot be applied to value <{obj}>");
            }
        }

        public static dynamic Evaluate(OperatorInfix op, dynamic left, dynamic right)
        {
            switch (op)
            {
                case OperatorInfix.Add when left is string && right is string:
                    return string.Concat(left, right);

                case OperatorInfix.Add when CheckNumeric(left, right):
                    return left + right;

                case OperatorInfix.Sub when CheckNumeric(left, right):
                    return left - right;

                case OperatorInfix.Mul when CheckNumeric(left, right):
                    return left * right;

                case OperatorInfix.Div when CheckNumeric(left, right):
                    return left / right;

                case OperatorInfix.Mod when CheckNumeric(left, right):
                    return left % right;

                case OperatorInfix.Lt when CheckNumeric(left, right):
                    return left < right;

                case OperatorInfix.LtEq when CheckNumeric(left, right):
                    return left <= right;

                case OperatorInfix.Gt when CheckNumeric(left, right):
                    return left > right;

                case OperatorInfix.GtEq when CheckNumeric(left, right):
                    return left >= right;

                case OperatorInfix.Eq:
                    return AreEqual(left, right);

                case OperatorInfix.Neq:
                    return !AreEqual(left, right);

                default:
                    throw new RuntimeErrorException(
                        $"Operator {op} cannot be applied to values " +
                        $"<{left}> and <{right}>");
            }
        }

        public static dynamic Evaluate(CommandType cmd, List<dynamic> args)
        {
            switch (cmd)
            {
                case CommandType.Push:
                    if (args[0] is List<dynamic> list)
                    {
                        var value = args[1];
                        list.Add(value);
                        return value;
                    }
                    else
                    {
                        throw new RuntimeErrorException(
                            $"<{args[0]}> not supported by push()");
                    }

                case CommandType.ReadLn:
                    {
                        if (args.Count > 0)
                        {
                            var prompt = string.Join("", args);
                            Console.Write(prompt);
                        }
                        var line = Console.ReadLine();
                        return line;
                    }

                case CommandType.WriteLn:
                    {
                        var line = string.Join("", args);
                        Console.WriteLine(line);
                        return null;
                    }

                default:
                    throw new RuntimeErrorException($"unknown command {cmd}");
            }
        }

        private static bool CheckNumeric(dynamic left, dynamic right)
        {
            return (left is int || left is double) && (right is int || right is double);
        }

        private static bool CheckIndexing(dynamic target)
        {
            return target is Dictionary<dynamic, dynamic> ||
                   target is List<dynamic> ||
                   target is string;
        }

        private static bool CheckIntIndexing(dynamic target, dynamic index)
        {
            return (target is List<dynamic> || target is string) && index is int;
        }

        private static bool AreEqual(dynamic left, dynamic right)
        {
            if (left == null && right == null)
            {
                return true;
            }
            else if (left == null)
            {
                return false;
            }
            else
            {
                return left.Equals(right);
            }
        }
    }
}
