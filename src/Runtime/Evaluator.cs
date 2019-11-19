﻿using Crisp.Ast;
using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Evaluator : IExpressionVisitor
    {
        Stack<object> stack = new Stack<object>();
        Program program;
        Environment environment;
        object @this;

        private Evaluator(Program program, Environment environment)
        {
            this.program = program;
            this.environment = environment;
        }

        public static object Run(Program program, Environment environment)
        {
            object result = Null.Instance;
            var evaluator = new Evaluator(program, environment);
            foreach (var expr in program.Expressions)
            {
                evaluator.Evaluate(expr);
                result = evaluator.stack.Pop();
            }
            return result;
        }

        public static object Run(Program program)
        {
            return Run(program, new Environment());
        }

        public void Invoke(Function function)
        {
            dynamic argArity = stack.Pop();
            if (function.Parameters.Count != argArity)
            {
                throw new Exception("Invoke airty mismatch");
            }

            var outerEnvironment = environment;
            environment = new Environment(environment);
            Bind(function.Parameters);
            EvaluateAsBlock(function.Body);
            environment = outerEnvironment;
        }

        public void Evaluate(IExpression expression)
        {
            expression.Accept(this);
        }

        void EvaluateAsBlock(List<IExpression> exprs)
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
            var entity = stack.Pop() as RecordInstance;
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
            var entity = stack.Pop() as RecordInstance;
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

        public void Visit(Call call)
        {
            if (program.Fns.TryGetValue(call.Name, out var fn))
            {
                foreach (var expr in call.Arguments)
                {
                    Evaluate(expr);
                }
                stack.Push(call.Arity);

                Invoke(program.Fns[call.Name]);
            }
            else if (program.Types.TryGetValue(call.Name, out var type))
            {
                if (call.Arguments.Count > 0)
                {
                    throw new RuntimeErrorException($"Record constructor {call.Name} must have no arguments");
                }

                var variables = new Dictionary<string, object>();
                foreach (var name in type.Variables)
                {
                    variables[name] = Null.Instance;
                }

                stack.Push(new RecordInstance(type.Name, variables));
            }
        }

        public void Visit(Condition condition)
        {
            // Invariant: No branch condition evaluated to true.
            foreach (var branch in condition.Branches)
            {
                Evaluate(branch.Condition);
                if (IsTrue())
                {
                    Evaluate(branch.Consequence);
                    return;
                }
            }

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

        public void Visit(Literal literal)
        {
            stack.Push(literal.Value ?? Null.Instance);
        }

        public void Visit(MessageSend messageSend)
        {
            Evaluate(messageSend.EntityExpr);
            if (!(stack.Pop() is RecordInstance entity))
            {
                throw new RuntimeErrorException(messageSend.Position, "message sent to non entity object");
            }

            foreach (var expr in messageSend.ArgumentExprs)
            {
                Evaluate(expr);
            }
            stack.Push(messageSend.ArgumentExprs.Count);

            var outerThis = @this;
            @this = entity;

            var record = program.Types[entity.RecordName];
            var fn = record.Functions[messageSend.Name];
            Invoke(fn);

            @this = outerThis;
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