using Crisp.Ast;
using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Evaluator : IExpressionVisitor
    {
        Stack<object> stack = new Stack<object>();
        Environment environment;

        public Evaluator(Environment environment)
        {
            this.environment = environment;
        }

        public void Push(object o)
        {
            stack.Push(o);
        }

        public object Pop()
        {
            return stack.Pop();
        }

        public void Invoke(Function function)
        {
            dynamic argArity = stack.Pop();
            if (function.Parameters.Count != argArity)
            {
                throw new Exception("Invoke airty mismatch");
            }

            var outerEnvironment = environment;
            environment = new Environment(function.Environment);
            Bind(function.Parameters);
            Evaluate(function.Body);
            environment = outerEnvironment;
        }

        public void Evaluate(IExpression expression)
        {
            expression.Accept(this);
        }

        void Bind(string name)
        {
            if (!environment.Create(name, stack.Pop()))
            {
                throw new RuntimeErrorException($"{name} already bound");
            }
        }

        void Bind(List<string> names)
        {
            for (var i = 0; i < names.Count; i++)
            {
                Bind(names[names.Count - i - 1]);
            }
        }

        void Set(string name)
        {
            if (!environment.Set(name, stack.Pop()))
            {
                throw new RuntimeErrorException(
                    $"Cannot assign value to unbound name <{name}>");
            }
        }

        bool IsTrue()
        {
            return IsTrue(stack.Pop());
        }

        static bool IsTrue(object x)
        {
            return !x.Equals(false) && !ReferenceEquals(x, Null.Instance);
        }

        static bool CheckNumeric(object x)
        {
            return x is int || x is double;
        }

        static bool CheckNumeric(dynamic left, dynamic right)
        {
            return CheckNumeric(left) && CheckNumeric(right);
        }

        public void Visit(AssignmentIdentifier assignmentIdentifier)
        {
            Evaluate(assignmentIdentifier.Value);
            var value = stack.Peek();
            Set(assignmentIdentifier.Target.Name);
            stack.Push(value);
        }

        public void Visit(AssignmentIndexing assignmentIndexing)
        {
            Evaluate(assignmentIndexing.Target.Indexable);
            var target = stack.Pop();

            Evaluate(assignmentIndexing.Target.Index);
            var index = stack.Pop();

            Evaluate(assignmentIndexing.Value);
            var value = stack.Pop();

            switch (target)
            {
                default:
                    throw new RuntimeErrorException("Set index on non-indexable object indexed.");
            }

            stack.Push(value);
        }

        public void Visit(AttributeAccess attributeAccess)
        {
            Evaluate(attributeAccess.Entity);
            var entity = stack.Pop() as Entity;
            if (entity == null)
            {
                throw new RuntimeErrorException("attribute access on non entity object");
            }

            if (entity.GetAttribute(attributeAccess.Name, out var value))
            {
                stack.Push(value);
            }
            else
            {
                throw new RuntimeErrorException($"attribute {attributeAccess.Name} not found");
            }
        }

        public void Visit(AttributeAssignment attributeAssignment)
        {
            Evaluate(attributeAssignment.Entity);
            var entity = stack.Pop() as Entity;
            if (entity == null)
            {
                throw new RuntimeErrorException("attribute assignment on non entity object");
            }

            Evaluate(attributeAssignment.Value);
            var value = stack.Pop();

            if (entity.SetAttribute(attributeAssignment.Name, value) == false)
            {
                throw new RuntimeErrorException(
                    $"attritue {attributeAssignment.Name} not found.");
            }

            stack.Push(value);
        }

        public void Visit(Block block)
        {
            if (block.Body.Count == 0)
            {
                stack.Push(Null.Instance);
            }
            else
            {
                var outerEnvironment = environment;
                environment = new Environment(environment);

                for (var i = 0; i < block.Body.Count; i++)
                {
                    Evaluate(block.Body[i]);

                    // Ignore all but the last evaluation.
                    if (i < block.Body.Count - 1)
                    {
                        stack.Pop();
                    }
                }

                environment = outerEnvironment;
            }
        }

        public void Visit(Branch branch)
        {
            Evaluate(branch.Condition);
            if (IsTrue())
            {
                Evaluate(branch.Consequence);
            }
            else
            {
                Evaluate(branch.Alternative);
            }
        }

        public void Visit(Call call)
        {
            Evaluate(call.FunctionExpression);
            var function = stack.Pop() as Runtime.Function;
            if (function == null)
            {
                throw new RuntimeErrorException(
                    call.Position,
                    "function call attempted on non function value");
            }

            foreach (var expr in call.ArgumentExpressions)
            {
                Evaluate(expr);
            }
            stack.Push(call.Arity);

            Invoke(function);
        }

        public void Visit(Command command)
        {
            var args = new List<object>();
            foreach (var expr in command.ArgumentExpressions)
            {
                Evaluate(expr);
                args.Add(stack.Pop());
            }

            switch (command.Type)
            {
                case CommandType.ReadLn:
                    {
                        if (args.Count > 0)
                        {
                            var prompt = string.Join("", args);
                            Console.Write(prompt);
                        }
                        var line = Console.ReadLine();
                        stack.Push(line);
                    }
                    break;

                case CommandType.WriteLn:
                    {
                        var line = string.Join("", args);
                        Console.WriteLine(line);
                        stack.Push(Null.Instance);
                    }
                    break;

                default:
                    throw new RuntimeErrorException($"unknown command {command.Type}");
            }
        }

        public void Visit(For @for)
        {
            Evaluate(@for.Start);
            dynamic start = stack.Pop();
            if (!CheckNumeric(start))
            {
                throw new RuntimeErrorException(
                    "for loop start expresion evaluate to a numeric value");
            }

            Evaluate(@for.End);
            dynamic end = stack.Pop();
            if (!CheckNumeric(end))
            {
                throw new RuntimeErrorException(
                    "for loop start expresion evaluate to a numeric value");
            }

            var outerEnvironment = environment;
            for (dynamic i = start; i <= end; i = i + 1)
            {
                environment = new Environment(outerEnvironment);
                environment.Create(@for.VariableName, i);
                Evaluate(@for.Body);
                stack.Pop();
            }

            environment = outerEnvironment;
            stack.Push(Null.Instance);
        }

        public void Visit(Identifier identifier)
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

        public void Visit(Ast.Function function)
        {
            stack.Push(
                new Runtime.Function(function.Parameters, function.Body, environment));
        }

        public void Visit(Indexing indexing)
        {
            Evaluate(indexing.Indexable);
            var target = stack.Pop();
            Evaluate(indexing.Index);
            var index = stack.Pop();

            switch (target)
            {
                case string s when index is int i:
                    if (i < 0 || i >= s.Length)
                    {
                        throw new RuntimeErrorException("Index out of bounds of string.");
                    }
                    stack.Push(s[i].ToString());
                    break;

                case string s:
                    throw new RuntimeErrorException("Strings must be indexed by integers.");

                default:
                    throw new RuntimeErrorException("Get index on non-indexable object indexed.");
            }
        }

        public void Visit(Let let)
        {
            Evaluate(let.Value);
            var value = stack.Peek();
            Bind(let.Identifier.Name);
            stack.Push(value);
        }

        public void Visit<T>(Literal<T> literal)
        {
            stack.Push(literal.Value);
        }

        public void Visit(LiteralNull literalNull)
        {
            stack.Push(Null.Instance);
        }

        public void Visit(MessageSend messageSend)
        {
            Evaluate(messageSend.EntityExpr);
            var entity = stack.Pop() as Entity;
            if (entity == null)
            {
                throw new RuntimeErrorException(
                    messageSend.Position,
                    "message sent to non entity object");
            }

            foreach (var expr in messageSend.ArgumentExprs)
            {
                Evaluate(expr);
            }
            stack.Push(entity);
            stack.Push(messageSend.ArgumentExprs.Count + 1);

            var messageHandled = entity.SendMessage(messageSend.Name, this);

            if (!messageHandled)
            {
                throw new RuntimeErrorException(
                    messageSend.Position,
                    $"error sending message '{messageSend.Name}'");
            }
        }

        public void Visit(OperatorBinary operatorBinary)
        {
            if (operatorBinary.Op == OperatorInfix.And)
            {
                Evaluate(operatorBinary.Left);
                if (IsTrue())
                {
                    Evaluate(operatorBinary.Right);
                    stack.Push(IsTrue());
                }
                else
                {
                    stack.Push(false);
                }
                return;
            }

            if (operatorBinary.Op == OperatorInfix.Or)
            {
                Evaluate(operatorBinary.Left);
                if (IsTrue())
                {
                    stack.Push(true);
                }
                else
                {
                    Evaluate(operatorBinary.Right);
                    stack.Push(IsTrue());
                }
                return;
            }

            Evaluate(operatorBinary.Left);
            dynamic left = stack.Pop();
            Evaluate(operatorBinary.Right);
            dynamic right = stack.Pop();

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

        public void Visit(OperatorUnary operatorUnary)
        {
            Evaluate(operatorUnary.Expression);
            dynamic obj = stack.Pop();

            switch (operatorUnary.Op)
            {
                case OperatorPrefix.Neg when CheckNumeric(obj):
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

        public void Visit(Ast.Record record)
        {
            var instance = new Runtime.Record(
                record.Variables,
                record.Functions.MapDictionary(
                    (name, fn) => new Runtime.Function(fn.Parameters, fn.Body, environment)));
            stack.Push(instance);
        }

        public void Visit(RecordConstructor recordConstructor)
        {
            Evaluate(recordConstructor.Record);
            var rec = stack.Pop() as Runtime.Record;
            if (rec == null)
            {
                throw new RuntimeErrorException(
                    recordConstructor.Position,
                    "Record construction requires a record object.");
            }

            var init = new Dictionary<string, object>();
            foreach (var name in recordConstructor.Initializers.Keys)
            {
                Evaluate(recordConstructor.Initializers[name]);
                init[name] = stack.Pop();
            }

            stack.Push(rec.Construct(init));
        }

        public void Visit(While @while)
        {
            for (Evaluate(@while.Guard); IsTrue(); Evaluate(@while.Guard))
            {
                Evaluate(@while.Body);
                stack.Pop();
            }

            stack.Push(Null.Instance);
        }
    }
}
