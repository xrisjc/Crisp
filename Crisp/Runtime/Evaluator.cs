using Crisp.Ast;
using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Evaluator : IExpressionVisitor
    {
        Stack<object> stack = new Stack<object>();
        Environment environment;
        object @this = null;

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
            EvaluateAsBlock(function.Body);
            environment = outerEnvironment;
        }

        public void Evaluate(IExpression expression)
        {
            expression.Accept(this);
        }

        public void EvaluateAsBlock(List<IExpression> exprs)
        {
            if (exprs.Count == 0)
            {
                stack.Push(Null.Instance);
            }
            else
            {
                var outerEnvironment = environment;
                environment = new Environment(environment);

                for (var i = 0; i < exprs.Count; i++)
                {
                    Evaluate(exprs[i]);

                    // Ignore all but the last evaluation.
                    if (i < exprs.Count - 1)
                    {
                        stack.Pop();
                    }
                }

                environment = outerEnvironment;
            }
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

        void Set(Identifier identifier)
        {
            if (!environment.Set(identifier.Name, stack.Pop()))
            {
                throw new RuntimeErrorException(
                    identifier.Position,
                    $"Cannot assign value to unbound name <{identifier.Name}>");
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
            Set(assignmentIdentifier.Target);
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
            EvaluateAsBlock(block.Body);
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
            var fn = new Function(function.Parameters, function.Body, environment);

            environment.Create(function.Name, fn);

            stack.Push(fn);
        }

        public void Visit(Literal literal)
        {
            stack.Push(literal.Value);
        }

        public void Visit(MessageSend messageSend)
        {
            Evaluate(messageSend.EntityExpr);
            var entity = stack.Pop() as Entity ?? throw new RuntimeErrorException(messageSend.Position, "message sent to non entity object");

            foreach (var expr in messageSend.ArgumentExprs)
            {
                Evaluate(expr);
            }
            stack.Push(messageSend.ArgumentExprs.Count);

            var outerThis = @this;
            @this = entity;

            var messageHandled = entity.SendMessage(messageSend.Name, this);

            @this = outerThis;

            if (!messageHandled)
            {
                throw new RuntimeErrorException(
                    messageSend.Position,
                    $"error sending message '{messageSend.Name}'");
            }
        }

        public void Visit(OperatorBinary operatorBinary)
        {
            if (operatorBinary.Tag == OperatorBinaryTag.And)
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

            if (operatorBinary.Tag == OperatorBinaryTag.Or)
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

            switch (operatorBinary.Tag)
            {
                case OperatorBinaryTag.Add when left is string && right is string:
                    stack.Push(string.Concat(left, right));
                    break;

                case OperatorBinaryTag.Add when CheckNumeric(left, right):
                    stack.Push(left + right);
                    break;

                case OperatorBinaryTag.Sub when CheckNumeric(left, right):
                    stack.Push(left - right);
                    break;

                case OperatorBinaryTag.Mul when CheckNumeric(left, right):
                    stack.Push(left * right);
                    break;

                case OperatorBinaryTag.Div when CheckNumeric(left, right):
                    stack.Push(left / right);
                    break;

                case OperatorBinaryTag.Mod when CheckNumeric(left, right):
                    stack.Push(left % right);
                    break;

                case OperatorBinaryTag.Lt when CheckNumeric(left, right):
                    stack.Push(left < right);
                    break;

                case OperatorBinaryTag.LtEq when CheckNumeric(left, right):
                    stack.Push(left <= right);
                    break;

                case OperatorBinaryTag.Gt when CheckNumeric(left, right):
                    stack.Push(left > right);
                    break;

                case OperatorBinaryTag.GtEq when CheckNumeric(left, right):
                    stack.Push(left >= right);
                    break;

                case OperatorBinaryTag.Eq when CheckNumeric(left, right):
                    // If left is int and right is double then left.Equals(right) may not
                    // be the same as right.Equals(left).  I suppose it's something to do with
                    // calinging Int32's Equals.  == seems to work for numbers, though.
                    stack.Push(left == right);
                    break;

                case OperatorBinaryTag.Eq:
                    stack.Push(left.Equals(right));
                    break;

                case OperatorBinaryTag.Neq:
                    stack.Push(!left.Equals(right));
                    break;

                default:
                    throw new RuntimeErrorException(
                        operatorBinary.Position,
                        $"Operator {operatorBinary.Tag} cannot be applied to values " +
                        $"<{left}> and <{right}>");
            }
        }

        public void Visit(OperatorUnary operatorUnary)
        {
            Evaluate(operatorUnary.Expression);
            dynamic obj = stack.Pop();

            switch (operatorUnary.Op)
            {
                case OperatorUnaryTag.Neg when CheckNumeric(obj):
                    stack.Push(-obj);
                    break;

                case OperatorUnaryTag.Not:
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

            environment.Create(record.Name, instance);

            stack.Push(instance);
        }

        public void Visit(RecordConstructor recordConstructor)
        {
            Evaluate(recordConstructor.Record);
            var rec = stack.Pop() as Record;
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

        public void Visit(This @this)
        {
            stack.Push(this.@this ?? Null.Instance);
        }

        public void Visit(Var var)
        {
            Evaluate(var.InitialValue);
            var value = stack.Peek();
            Bind(var.Name);
            stack.Push(value);
        }

        public void Visit(While @while)
        {
            for (Evaluate(@while.Guard); IsTrue(); Evaluate(@while.Guard))
            {
                EvaluateAsBlock(@while.Body);
                stack.Pop();
            }

            stack.Push(Null.Instance);
        }

        public void Visit(Write command)
        {
            foreach (var expr in command.Arguments)
            {
                Evaluate(expr);
                Console.Write(stack.Pop());
            }
            stack.Push(Null.Instance);
        }
    }
}
