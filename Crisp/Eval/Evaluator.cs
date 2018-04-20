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
                        var index = e.Target.Index.Evaluate(environment);
                        var value = e.Value.Evaluate(environment);
                        SetIndex(target, index, value);
                        return value;
                    }

                case AssignmentIdentifier e:
                    return environment.Set(e.Target.Name, e.Value.Evaluate(environment));

                case AssignmentMember e:
                    {
                        var obj = e.Target.Expression.Evaluate(environment);
                        var name = e.Target.MemberIdentifier.Name;
                        var value = e.Value.Evaluate(environment);
                        MemberSet(obj, name, value);
                        return value;
                    }

                case Block block:
                    {
                        var localEnvironment = new Environment(environment);
                        return block.Body.Evaluate(localEnvironment).LastOrDefault() ?? Null.Instance;
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
                when call.FunctionExpression.Evaluate(environment) is Function function:
                    if (function.Arity == call.Arity)
                    {
                        var arguments = call.ArgumentExpressions.Evaluate(environment).ToList();
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
                    return Eval.Evaluator.Evaluate(
                        command.Type,
                        command.ArgumentExpressions.Evaluate(environment).ToList());

                case MemberCall call
                when call.Member.Expression.Evaluate(environment) is RecordInstance record:
                    {
                        var fn = record.GetMemberFunction(call.Member.MemberIdentifier.Name);
                        if (fn.Arity != call.Arity + 1) // + 1 for "this" argument
                        {
                            throw new RuntimeErrorException("function arity mismatch");
                        }

                        var arguments = call.ArgumentExpressions.Evaluate(environment).ToList();
                        arguments.Add(record); // the "this" argument is at the end
                        return fn.Call(arguments);
                    }

                case MemberCall call:
                    throw new RuntimeErrorException("method call must be on a record instance");

                case NamedFunction fn:
                    return environment.Create(fn.Name.Name, new Function(fn, environment));

                case Member m:
                    {
                        var obj = m.Expression.Evaluate(environment);
                        return MemberGet(obj, m.MemberIdentifier.Name);
                    }

                case Ast.Function fn:
                    return new Function(fn, environment);

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
                        var index = indexing.Index.Evaluate(environment);
                        return GetIndex(target, index);
                    }

                case List list:
                    return list.Initializers.Evaluate(environment).ToList();

                case LiteralBool literal:
                    return literal.Value;

                case LiteralInt literal:
                    return literal.Value;

                case LiteralDouble literal:
                    return literal.Value;

                case LiteralString literal:
                    return literal.Value;

                case LiteralNull literal:
                    return Null.Instance;

                case Map map:
                    return map.Initializers.Evaluate(environment).CreateDictionary();

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
                                    and.Token,
                                    "Right hand side of 'and' must be Boolean.");
                            }
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                and.Token,
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
                                    or.Token,
                                    "Right hand side of 'or' must be Boolean.");
                            }
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                or.Token,
                                "Left hand side of 'or' must be Boolean.");
                        }
                    }

                case OperatorBinary operatorBinary:
                    return Evaluate(operatorBinary, environment);

                case OperatorUnary operatorUnary:
                    return Evaluate(operatorUnary, environment);

                case Ast.Record rec:
                    return new Record(
                        rec.Variables.Select(x => x.Name).ToList(),
                        rec.Functions.ToDictionary(
                            nf => nf.Name.Name,
                            nf => new Function(nf, environment)));

                case RecordConstructor ctor
                when ctor.Record.Evaluate(environment) is Record rec:
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
                                return Null.Instance;
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

        public static IEnumerable<dynamic> Evaluate(
            this IEnumerable<IExpression> expressions,
            Environment environment)
        {
            return from expression in expressions
                   select expression.Evaluate(environment);
        }

        public static IEnumerable<(dynamic, dynamic)> Evaluate(
            this IEnumerable<(IExpression, IExpression)> expressions,
            Environment environment)
        {
            return from e in expressions
                   select (e.Item1.Evaluate(environment), e.Item2.Evaluate(environment));
        }

        public static dynamic Evaluate(OperatorUnary operatorUnary, Environment environment)
        {
            var obj = operatorUnary.Expression.Evaluate(environment);

            switch (operatorUnary.Op)
            {
                case OperatorPrefix.Neg when obj is int || obj is double: return -obj;
                case OperatorPrefix.Not when obj is bool: return !obj;
                default:
                    throw new RuntimeErrorException(
                        operatorUnary.Token,
                        $"Operator {operatorUnary.Op} cannot be applied to value <{obj}>");
            }
        }

        public static dynamic Evaluate(OperatorBinary operatorBinary, Environment environment)
        {
            var left = operatorBinary.Left.Evaluate(environment);
            var right = operatorBinary.Right.Evaluate(environment);

            switch (operatorBinary.Op)
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
                        operatorBinary.Token,
                        $"Operator {operatorBinary.Token} cannot be applied to values " +
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

        public static dynamic GetIndex(dynamic target, dynamic index)
        {
            switch (target)
            {
                case String s when index is int i:
                    if (i < 0 || i >= s.Length)
                    {
                        throw new RuntimeErrorException("Index out of bounds of string.");
                    }
                    return s[i].ToString();

                case String s:
                    throw new RuntimeErrorException("Strings must be indexed by integers.");

                case List<dynamic> l when index is int i:
                    if (i < 0 || i >= l.Count)
                    {
                        throw new RuntimeErrorException("Index out of bounds of list.");
                    }
                    return l[i];

                case List<dynamic> l:
                    throw new RuntimeErrorException("Lists must be indexed by integers.");

                case Dictionary<dynamic, dynamic> d:
                    if (!d.ContainsKey(index))
                    {
                        throw new RuntimeErrorException("Key not found in map.");
                    }
                    return d[index];

                default:
                    throw new RuntimeErrorException("Get index on non-indexable object indexed.");
            }
        }

        public static void SetIndex(dynamic target, dynamic index, dynamic value)
        {
            switch (target)
            {
                case List<dynamic> l when index is int i:
                    if (i < 0 || i >= l.Count)
                    {
                        throw new RuntimeErrorException("Index out of bounds of list.");
                    }
                    l[i] = value;
                    break;

                case List<dynamic> l:
                    throw new RuntimeErrorException("Lists must be indexed by integers.");

                case Dictionary<dynamic, dynamic> d:
                    d[index] = value;
                    break;

                default:
                    throw new RuntimeErrorException("Set index on non-indexable object indexed.");
            }
        }

        public static dynamic MemberGet(dynamic obj, string name)
        {
            switch (obj)
            {
                case RecordInstance ri:
                    if (ri.MemberGet(name, out var value))
                    {
                        return value;
                    }
                    else
                    {
                        throw new RuntimeErrorException($"cannot get member {name}");
                    }

                default:
                    throw new RuntimeErrorException("object doesn't support member getting");
            }
        }

        public static void MemberSet(dynamic obj, string name, dynamic value)
        {
            switch (obj)
            {
                case RecordInstance ri:
                    if (ri.MemberSet(name, value) == false)
                    {
                        throw new RuntimeErrorException($"Member {name} not found.");
                    }
                    break;

                default:
                    throw new RuntimeErrorException("object doesn't support member setting");
            }
        }

        private static bool CheckNumeric(dynamic left, dynamic right)
        {
            return (left is int || left is double) && (right is int || right is double);
        }

        private static bool AreEqual(dynamic left, dynamic right)
        {
            if (left is Null && right is Null)
            {
                return true;
            }
            else if (left is Null)
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
