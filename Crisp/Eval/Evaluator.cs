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
                    {
                        var value = e.Value.Evaluate(environment);
                        if (environment.Set(e.Target.Name, value))
                        {
                            return value;
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                e.Target.Position,
                                $"Cannot assign value to unbound name <{e.Target.Name}>");
                        }
                    }

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
                    if (IsTrue(branch.Condition.Evaluate(environment)))
                    {
                        return branch.Consequence.Evaluate(environment);
                    }
                    else
                    {
                        return branch.Alternative.Evaluate(environment);
                    }

                case Call call:
                    {
                        var function = call.FunctionExpression.Evaluate(environment) as Function;
                        if (function == null)
                        {
                            throw new RuntimeErrorException(
                                call.Position,
                                "function call attempted on non function value");
                        }

                        if (function.Parameters.Count != call.Arity)
                        {
                            throw new RuntimeErrorException(
                                call.Position,
                                "function arity mismatch");
                        }
                        var arguments = call.ArgumentExpressions.Evaluate(environment).ToList();

                        var localEnvironment = new Environment(function.Environment);
                        for (int i = 0; i < function.Parameters.Count; i++)
                        {
                            localEnvironment.Create(function.Parameters[i], arguments[i]);
                        }

                        return function.Body.Evaluate(localEnvironment);
                    }

                case Command command:
                    return Eval.Evaluator.Evaluate(
                        command.Type,
                        command.ArgumentExpressions.Evaluate(environment).ToList());

                case MemberCall call:
                    {
                        var record = call.Member.Expression.Evaluate(environment) as RecordInstance;
                        if (record == null)
                        {
                            throw new RuntimeErrorException(
                                call.Position,
                                "method call must be on a record instance");
                        }

                        var function = record.GetMemberFunction(call.Member.MemberIdentifier.Name);
                        if (function.Parameters.Count != call.Arity + 1) // + 1 for "this" argument
                        {
                            throw new RuntimeErrorException(
                                call.Position,
                                "method call arity mismatch");
                        }
                        var arguments = call.ArgumentExpressions.Evaluate(environment).ToList();
                        arguments.Add(record); // the "this" argument is at the end

                        var localEnvironment = new Environment(function.Environment);
                        for (int i = 0; i < function.Parameters.Count; i++)
                        {
                            localEnvironment.Create(function.Parameters[i], arguments[i]);
                        }

                        return function.Body.Evaluate(localEnvironment);
                    }

                case Member m:
                    {
                        var obj = m.Expression.Evaluate(environment);
                        return MemberGet(obj, m.MemberIdentifier.Name);
                    }

                case Ast.Function fn:
                    return new Function(fn.Parameters, fn.Body, environment);

                case Let let:
                    {
                        var value = let.Value.Evaluate(environment);
                        if (environment.Create(let.Identifier.Name, value))
                        {
                            return value;
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                let.Identifier.Position,
                                $"Name <{let.Identifier.Name}> was already bound previously.");
                        }
                    }

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
                    {
                        if (environment.Get(identifier.Name, out var value))
                        {
                            return value;
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                identifier.Position,
                                $"<{identifier.Name}> not bound to a value.");
                        }
                    }

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

                case OperatorBinary and when and.Op == OperatorInfix.And:
                    return IsTrue(and.Left.Evaluate(environment)) && IsTrue(and.Right.Evaluate(environment));

                case OperatorBinary or when or.Op == OperatorInfix.Or:
                    return IsTrue(or.Left.Evaluate(environment)) || IsTrue(or.Right.Evaluate(environment));

                case OperatorBinary operatorBinary:
                    return Evaluate(operatorBinary, environment);

                case OperatorUnary operatorUnary:
                    return Evaluate(operatorUnary, environment);

                case Ast.Record rec:
                    return new Record(
                        rec.Variables,
                        rec.Functions.MapDictionary(
                            (name, fn) => new Function(fn.Parameters, fn.Body, environment)));

                case RecordConstructor ctor:
                    {
                        var rec = ctor.Record.Evaluate(environment) as Record;
                        if (rec == null)
                        {
                            throw new RuntimeErrorException(
                                ctor.Position,
                                "Record construction requires a record object.");
                        }
                        var members = ctor.Initializers.MapDictionary((name, expr) => expr.Evaluate(environment));
                        return rec.Construct(members);
                    }

                case While @while:
                    while (IsTrue(@while.Guard.Evaluate(environment)))
                    {
                        @while.Body.Evaluate(environment);
                    }
                    return Null.Instance;

                case For @for:
                    {
                        var start = @for.Start.Evaluate(environment);
                        CheckNumeric(start);
                        var end = @for.End.Evaluate(environment);
                        CheckNumeric(end);
                        for (var i = start; i <= end; i = i + 1)
                        {
                            var localEnvironment = new Environment(environment);
                            localEnvironment.Create(@for.VariableName, i);
                            @for.Body.Evaluate(localEnvironment);
                        }
                    }
                    return Null.Instance;

                case ForIn forIn:
                    switch (forIn.Sequence.Evaluate(environment))
                    {
                        case List<object> list:
                            foreach (var x in list)
                            {
                                var localEnvironment = new Environment(environment);
                                localEnvironment.Create(forIn.VariableName, x);
                                forIn.Body.Evaluate(localEnvironment);
                            }
                            break;

                        default:
                            throw new RuntimeErrorException(
                                $"For in loops must have an enumerable object.");
                    }
                    return Null.Instance;

                default:
                    throw new RuntimeErrorException(
                        $"Unsupported AST node {expression}");
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
                case OperatorPrefix.Neg when obj is int || obj is double:
                    return -obj;
                case OperatorPrefix.Not:
                    return !IsTrue(obj);
                default:
                    throw new RuntimeErrorException(
                        operatorUnary.Position,
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

                case OperatorInfix.Eq when CheckNumeric(left, right):
                    // If left is int and right is double then left.Equals(right) may not
                    // be the same as right.Equals(left).  I suppose it's something to do with
                    // calinging Int32's Equals.  == seems to work for numbers, though.
                    return left == right;

                case OperatorInfix.Eq:
                    return left.Equals(right);

                case OperatorInfix.Neq:
                    return !left.Equals(right);

                default:
                    throw new RuntimeErrorException(
                        operatorBinary.Position,
                        $"Operator {operatorBinary.Op} cannot be applied to values " +
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
                        return Null.Instance;
                    }

                default:
                    throw new RuntimeErrorException($"unknown command {cmd}");
            }
        }

        public static bool IsTrue(object x)
        {
            return !x.Equals(false) && !ReferenceEquals(x, Null.Instance);
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

        private static bool CheckNumeric(dynamic x)
        {
            return x is int || x is double;
        }

        private static bool CheckNumeric(dynamic left, dynamic right)
        {
            return CheckNumeric(left) && CheckNumeric(right);
        }
    }
}
