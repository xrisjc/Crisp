using Crisp.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crisp.Eval
{
    static class Evaluator
    {
        public static void Evaluate(this IExpression expression, Stack<dynamic> stack, Environment environment)
        {
            switch (expression)
            {
                case AssignmentIndexing e:
                    e.Target.Indexable.Evaluate(stack, environment);
                    e.Target.Index.Evaluate(stack, environment);
                    e.Value.Evaluate(stack, environment);
                    SetIndex(stack);
                    break;

                case AssignmentIdentifier e:
                    {
                        e.Value.Evaluate(stack, environment);
                        var value = stack.Pop();
                        if (environment.Set(e.Target.Name, value))
                        {
                            stack.Push(value);
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                e.Target.Position,
                                $"Cannot assign value to unbound name <{e.Target.Name}>");
                        }
                    }
                    break;

                case AssignmentMember e:
                    e.Target.Expression.Evaluate(stack, environment);
                    stack.Push(e.Target.MemberIdentifier.Name);
                    e.Value.Evaluate(stack, environment);
                    MemberSet(stack);
                    break;

                case Block block:
                    {
                        var localEnvironment = new Environment(environment);
                        dynamic result = Null.Instance;
                        foreach (var expr in block.Body)
                        {
                            expr.Evaluate(stack, localEnvironment);
                            result = stack.Pop();
                        }
                        stack.Push(result);
                    }
                    break;

                case Branch branch:
                    branch.Condition.Evaluate(stack, environment);
                    if (IsTrue(stack.Pop()))
                    {
                        branch.Consequence.Evaluate(stack, environment);
                    }
                    else
                    {
                        branch.Alternative.Evaluate(stack, environment);
                    }
                    break;

                case Call call:
                    {
                        call.FunctionExpression.Evaluate(stack, environment);
                        var value = stack.Pop();
                        if (value is Function function)
                        {
                            if (function.Arity == call.Arity)
                            {
                                var arguments = new List<dynamic>();
                                foreach (var arg in call.ArgumentExpressions)
                                {
                                    arg.Evaluate(stack, environment);
                                    arguments.Add(stack.Pop());
                                }
                                function.Call(stack, arguments);
                            }
                            else
                            {
                                throw new RuntimeErrorException("function arity mismatch");
                            }
                        }
                        else
                        {
                            throw new RuntimeErrorException("function call attempted on non function value");
                        }
                    }
                    break;

                case Command command:
                    {
                        var arguments = new List<dynamic>();
                        foreach (var arg in command.ArgumentExpressions)
                        {
                            arg.Evaluate(stack, environment);
                            arguments.Add(stack.Pop());
                        }
                        stack.Push(Eval.Evaluator.Evaluate(command.Type, arguments));
                    }
                    break;

                case MemberCall call:
                    {
                        call.Member.Expression.Evaluate(stack, environment);
                        dynamic value = stack.Pop();
                        if (value is RecordInstance record)
                        {
                            var fn = record.GetMemberFunction(call.Member.MemberIdentifier.Name);
                            if (fn.Arity != call.Arity + 1) // + 1 for "this" argument
                            {
                                throw new RuntimeErrorException("function arity mismatch");
                            }

                            var arguments = new List<dynamic>(call.ArgumentExpressions.Count + 1);
                            foreach (var arg in call.ArgumentExpressions)
                            {
                                arg.Evaluate(stack, environment);
                                arguments.Add(stack.Pop());
                            }
                            arguments.Add(record); // the "this" argument is at the end
                            fn.Call(stack, arguments);
                        }
                        else
                        {
                            throw new RuntimeErrorException("method call must be on a record instance");
                        }
                    }
                    break;

                case Member m:
                    m.Expression.Evaluate(stack, environment);
                    stack.Push(MemberGet(stack.Pop(), m.MemberIdentifier.Name));
                    break;

                case Ast.Function fn:
                    stack.Push(new Function(fn.Parameters, fn.Body, environment));
                    break;

                case Let let:
                    {
                        let.Value.Evaluate(stack, environment);
                        var value = stack.Pop();
                        if (environment.Create(let.Identifier.Name, value))
                        {
                            stack.Push(value);
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                let.Identifier.Position,
                                $"Name <{let.Identifier.Name}> was already bound previously.");
                        }
                    }
                    break;

                case Len len:
                    {
                        len.Expression.Evaluate(stack, environment);
                        var obj = stack.Pop();
                        if (obj is Dictionary<dynamic, dynamic> || obj is List<dynamic>)
                        {
                            stack.Push(obj.Count);
                        }
                        else
                        {
                            throw new RuntimeErrorException("unsupported object passed to len()");
                        }
                    }
                    break;

                case Identifier identifier:
                    {
                        if (environment.Get(identifier.Name, out var value))
                        {
                            stack.Push(value);
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                identifier.Position,
                                $"<{identifier.Name}> not bound to a value.");
                        }
                    }
                    break;

                case Indexing indexing:
                    indexing.Indexable.Evaluate(stack, environment);
                    indexing.Index.Evaluate(stack, environment);
                    GetIndex(stack);
                    break;

                case List list:
                    {
                        var l = new List<dynamic>();
                        foreach (var x in list.Initializers)
                        {
                            x.Evaluate(stack, environment);
                            l.Add(stack.Pop());
                        }
                        stack.Push(l);
                    }
                    break;

                case LiteralBool literal:
                    stack.Push(literal.Value);
                    break;

                case LiteralInt literal:
                    stack.Push(literal.Value);
                    break;

                case LiteralDouble literal:
                    stack.Push(literal.Value);
                    break;

                case LiteralString literal:
                    stack.Push(literal.Value);
                    break;

                case LiteralNull literal:
                    stack.Push(Null.Instance);
                    break;

                case Map map:
                    {
                        var m = new Dictionary<dynamic, dynamic>();
                        foreach (var init in map.Initializers)
                        {
                            init.Item1.Evaluate(stack, environment);
                            init.Item2.Evaluate(stack, environment);
                            var value = stack.Pop();
                            var key = stack.Pop();
                            m[key] = value;
                        }
                        stack.Push(m);
                    }
                    break;

                case OperatorBinary and when and.Op == OperatorInfix.And:
                    and.Left.Evaluate(stack, environment);
                    if (IsTrue(stack.Pop()))
                    {
                        and.Right.Evaluate(stack, environment);
                        stack.Push(IsTrue(stack.Pop()));
                    }
                    else
                    {
                        stack.Push(false);
                    }
                    break;

                case OperatorBinary or when or.Op == OperatorInfix.Or:
                    or.Left.Evaluate(stack, environment);
                    if (IsTrue(stack.Pop()))
                    {
                        stack.Push(true);
                    }
                    else
                    {
                        or.Right.Evaluate(stack, environment);
                        stack.Push(IsTrue(stack.Pop()));
                    }
                    break;

                case OperatorBinary operatorBinary:
                    operatorBinary.Left.Evaluate(stack, environment);
                    operatorBinary.Right.Evaluate(stack, environment);
                    Evaluate(operatorBinary, stack);
                    break;

                case OperatorUnary operatorUnary:
                    operatorUnary.Expression.Evaluate(stack, environment);
                    Evaluate(operatorUnary, stack);
                    break;

                case Ast.Record rec:
                    stack.Push(new Record(
                        rec.Variables,
                        rec.Functions.MapDictionary(
                            (name, fn) => new Function(fn.Parameters, fn.Body, environment))));
                    break;

                case RecordConstructor ctor:
                    {
                        ctor.Record.Evaluate(stack, environment);
                        var value = stack.Pop();
                        if (value is Record rec)
                        {
                            var initializers = new Dictionary<string, dynamic>();
                            foreach (var init in ctor.Initializers)
                            {
                                init.Value.Evaluate(stack, environment);
                                initializers.Add(init.Key, stack.Pop());
                            }
                            stack.Push(rec.Construct(initializers));
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                $"Record construction requires a record object.");
                        }
                    }
                    break;

                case While @while:
                    @while.Guard.Evaluate(stack, environment);
                    while (IsTrue(stack.Pop()))
                    {
                        @while.Body.Evaluate(stack, environment);
                        stack.Pop();
                        @while.Guard.Evaluate(stack, environment);
                    }
                    stack.Push(Null.Instance);
                    break;

                case For @for:
                    {
                        @for.Start.Evaluate(stack, environment);
                        var start = stack.Pop();
                        CheckNumeric(start);
                        @for.End.Evaluate(stack, environment);
                        var end = stack.Pop();
                        CheckNumeric(end);
                        for (var i = start; i <= end; i = i + 1)
                        {
                            var localEnvironment = new Environment(environment);
                            localEnvironment.Create(@for.VariableName, i);
                            @for.Body.Evaluate(stack, localEnvironment);
                            stack.Pop();
                        }
                    }
                    stack.Push(Null.Instance);
                    break;

                case ForIn forIn:
                    {
                        forIn.Sequence.Evaluate(stack, environment);
                        switch (stack.Pop())
                        {
                            case List<object> list:
                                foreach (var x in list)
                                {
                                    var localEnvironment = new Environment(environment);
                                    localEnvironment.Create(forIn.VariableName, x);
                                    forIn.Body.Evaluate(stack, localEnvironment);
                                    stack.Pop();
                                }
                                break;

                            default:
                                throw new RuntimeErrorException(
                                    $"For in loops must have an enumerable object.");
                        }
                    }
                    stack.Push(Null.Instance);
                    break;

                default:
                    throw new RuntimeErrorException(
                        $"Unsupported AST node {expression}");
            }
        }

        public static void Evaluate(OperatorUnary operatorUnary, Stack<dynamic> stack)
        {
            var obj = stack.Pop();

            switch (operatorUnary.Op)
            {
                case OperatorPrefix.Neg when obj is int || obj is double:
                    stack.Push(-obj);
                    break;
                case OperatorPrefix.Not:
                    stack.Push(!IsTrue(obj));
                    break;
                default:
                    throw new RuntimeErrorException(
                        operatorUnary.Position,
                        $"Operator {operatorUnary.Op} cannot be applied to value <{obj}>");
            }
        }

        public static void Evaluate(OperatorBinary operatorBinary, Stack<dynamic> stack)
        {
            var right = stack.Pop();
            var left = stack.Pop();

            switch (operatorBinary.Op)
            {
                case OperatorInfix.Add when left is string && right is string:
                    stack.Push(string.Concat(left, right));
                    break;

                case OperatorInfix.Add when CheckNumeric(left, right):
                    stack.Push(left + right);
                    break;

                case OperatorInfix.Sub when CheckNumeric(left, right):
                    stack.Push(left - right);
                    break;

                case OperatorInfix.Mul when CheckNumeric(left, right):
                    stack.Push(left * right);
                    break;

                case OperatorInfix.Div when CheckNumeric(left, right):
                    stack.Push(left / right);
                    break;

                case OperatorInfix.Mod when CheckNumeric(left, right):
                    stack.Push(left % right);
                    break;

                case OperatorInfix.Lt when CheckNumeric(left, right):
                    stack.Push(left < right);
                    break;

                case OperatorInfix.LtEq when CheckNumeric(left, right):
                    stack.Push(left <= right);
                    break;

                case OperatorInfix.Gt when CheckNumeric(left, right):
                    stack.Push(left > right);
                    break;

                case OperatorInfix.GtEq when CheckNumeric(left, right):
                    stack.Push(left >= right);
                    break;

                case OperatorInfix.Eq when CheckNumeric(left, right):
                    // If left is int and right is double then left.Equals(right) may not
                    // be the same as right.Equals(left).  I suppose it's something to do with
                    // calinging Int32's Equals.  == seems to work for numbers, though.
                    stack.Push(left == right);
                    break;

                case OperatorInfix.Eq:
                    stack.Push(left.Equals(right));
                    break;

                case OperatorInfix.Neq:
                    stack.Push(!left.Equals(right));
                    break;

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

        public static void GetIndex(Stack<dynamic> stack)
        {
            var index = stack.Pop();
            var target = stack.Pop();

            switch (target)
            {
                case String s when index is int i:
                    if (i < 0 || i >= s.Length)
                    {
                        throw new RuntimeErrorException("Index out of bounds of string.");
                    }
                    stack.Push(s[i].ToString());
                    break;

                case String s:
                    throw new RuntimeErrorException("Strings must be indexed by integers.");

                case List<dynamic> l when index is int i:
                    if (i < 0 || i >= l.Count)
                    {
                        throw new RuntimeErrorException("Index out of bounds of list.");
                    }
                    stack.Push(l[i]);
                    break;

                case List<dynamic> l:
                    throw new RuntimeErrorException("Lists must be indexed by integers.");

                case Dictionary<dynamic, dynamic> d:
                    if (!d.ContainsKey(index))
                    {
                        throw new RuntimeErrorException("Key not found in map.");
                    }
                    stack.Push(d[index]);
                    break;

                default:
                    throw new RuntimeErrorException("Get index on non-indexable object indexed.");
            }
        }

        public static void SetIndex(Stack<dynamic> stack)
        {
            var value = stack.Pop();
            var index = stack.Pop();
            var target = stack.Pop();

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

            stack.Push(value);
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

        public static void MemberSet(Stack<dynamic> stack)
        {
            var value = stack.Pop();
            var name = stack.Pop();
            var obj = stack.Pop();

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

            stack.Push(value);
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
