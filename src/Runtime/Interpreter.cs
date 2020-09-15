using System;
using System.Collections.Generic;
using System.Linq;
using Crisp.Ast;

namespace Crisp.Runtime
{
    class Interpreter : IExpressionVisitor<Obj>
    {
        public System System { get; }
        public Environment Globals { get; }
        public Environment Environment { get; }
        public Obj? Self { get; }

        public Interpreter(
            System system,
            Environment globals,
            Environment environment,
            Obj? self)
        {
            System = system;
            Globals = globals;
            Environment = environment;
            Self = self;
        }

        public Interpreter(System system, Environment globals)
            : this(system, globals, globals, null)
        {
        }

        Interpreter Push()
            => new Interpreter(System, Globals, new Environment(Environment), Self);

        Interpreter PushWith(Obj self)
            => new Interpreter(System, Globals, new Environment(Environment), self);

        static int? AsIndex(Obj obj)
        {
            if (obj.Value is double n)
            {
                var floor = Math.Floor(n);
                if (floor == Math.Ceiling(n))
                {
                    var index = (int)floor;
                    return index;
                }
            }
            return null;
        }

        Obj LookupProperty(Obj obj, Obj key)
        {
            if (obj.Value is List<Obj> list && AsIndex(key) is int index)
            {
                // TODO: Bounds check.
                return list[index];
            }
            
            return obj.LookupProperty(key) ?? System.Null;
        }

        void SetProperty(Obj obj, Obj key, Obj value)
        {
            if (obj.Value is List<Obj> list && AsIndex(key) is int index)
            {
                // TODO: Bounds check.
                list[index] = value;
                return;
            }

            obj.SetProperty(key, value);
        }

        bool IsTruthy(Obj obj)
            => !obj.Equals(System.False) && !obj.Equals(System.Null);

        public Obj Evaluate(IExpression expression)
            => expression.Accept(this);

        public Obj Visit(AssignmentIdentifier ai)
        {
            var result = Evaluate(ai.Value);
            if (!Environment.Set(ai.Target.Name, result))
                throw new RuntimeErrorException(
                    ai.Target.Position,
                    $"Cannot assign, <{ai.Target.Name}> is unbound");
            return result;
        }

        public Obj Visit(AssignmentIndex ai)
        {
            var target = Evaluate(ai.Index.Target);
            var key = Evaluate(ai.Index.Key);
            var value = Evaluate(ai.Value);
            SetProperty(target, key, value);
            return value;
        }

        public Obj Visit(AssignmentRefinement ar)
        {
            var target = Evaluate(ar.Refinement.Target);
            var key = System.Create(ar.Refinement.Name);
            var value = Evaluate(ar.Value);
            SetProperty(target, key, value);
            return value;
        }

        public Obj Visit(Block block)
        {
            var result = System.Null;
            {
                var interpreter = Push();
                foreach (var e in block.Body)
                    result = interpreter.Evaluate(e);
            }
            return result;
        }

        public Obj Visit(Call call)
        {
            Obj? self;
            Obj key, value;
            switch (call.Target)
            {
                case Ast.Index index:
                    self = Evaluate(index.Target);
                    key = Evaluate(index.Key);
                    value = LookupProperty(self, key);
                    break;
                case Refinement rfnt:
                    self = Evaluate(rfnt.Target);
                    key = System.Create(rfnt.Name);
                    value = LookupProperty(self, key);
                    break;
                default:
                    self = Self;
                    value = Evaluate(call.Target);
                    break;
            }

            if (value.Value is Callable callable)
            {
                var args = from arg in call.Arguments
                            select Evaluate(arg);
                return callable(this, self, args.ToArray());
            }
            else
            {
                throw new RuntimeErrorException(
                    call.Position,
                    $"Cannot call non-callable object <{value}>.");
            }
        }

        public Obj Visit(Create c)
        {
            var prototype = Evaluate(c.Prototype);
            return new Obj(prototype);
        }

        public Obj Visit(Conditional c)
        {
            foreach (var (condition, consequence) in c.Branches)
            {
                if (IsTruthy(Evaluate(condition)))
                    return Evaluate(consequence);
            }

            if (c.ElseBlock != null)
                return Evaluate(c.ElseBlock);

            return System.Null;
        }

        public Obj Visit(Ast.Index index)
        {
            var obj = Evaluate(index.Target);
            var key = Evaluate(index.Key);
            return LookupProperty(obj, key);
        }

        public Obj Visit(Function function)
        {
            var closure = Environment;

            Callable callable = (Interpreter interpreter, Obj? self, Obj[] arguments) =>
            {
                var environment = new Environment(closure);
                interpreter = new Interpreter(
                    interpreter.System,
                    interpreter.Globals,
                    environment,
                    self);
                var parameters = function.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    Obj value;
                    if (i < arguments.Length)
                        value = arguments[i];
                    else
                        value = interpreter.System.Null;
                    
                    if (!environment.Create(parameters[i].Name, value))
                        throw new RuntimeErrorException(
                            parameters[i].Position,
                            $"Parameter {parameters[i].Name} already bound.");
                }
                return interpreter.Evaluate(function.Body);
            };

            return System.Create(callable);
        }

        public Obj Visit(Identifier identifier)
            => Environment.Get(identifier.Name) ??
                    throw new RuntimeErrorException(
                        identifier.Position,
                        $"Identifier <{identifier.Name}> unbound");

        public Obj Visit(LiteralBool literalBool)
            => System.Create(literalBool.Value);

        public Obj Visit(LiteralNull ln)
            => System.Null;

        public Obj Visit(LiteralNumber number)
            => System.Create(number.Value);

        public Obj Visit(LiteralString literalString)
            => System.Create(literalString.Value);

        public Obj Visit(LiteralList literalList)
        {
            var items = from expr in literalList.Items
                        select Evaluate(expr);
            return System.Create(items.ToList());
        }

        public Obj Visit(OperatorBinary op)
        {
            if (op.Tag == OperatorBinaryTag.And)
                return System.Create(
                        IsTruthy(Evaluate(op.Left)) &&
                        IsTruthy(Evaluate(op.Right)));
            
            if (op.Tag == OperatorBinaryTag.Or)
                return System.Create(
                        IsTruthy(Evaluate(op.Left)) ||
                        IsTruthy(Evaluate(op.Right)));

            var left = Evaluate(op.Left);
            var right = Evaluate(op.Right);

            if (op.Tag == OperatorBinaryTag.Is)
                return System.Create(left.Is(right));
            else if (op.Tag == OperatorBinaryTag.Eq)
                return System.Create(left.Equals(right));
            else if (op.Tag == OperatorBinaryTag.Neq)
                return System.Create(!left.Equals(right));

            return (op.Tag, left.Value, right.Value) switch
            {
                (OperatorBinaryTag.Add, double l, double r)
                    => System.Create(l + r),
                
                (OperatorBinaryTag.Sub, double l, double r)
                    => System.Create(l - r),
                
                (OperatorBinaryTag.Mul, double l, double r)
                    => System.Create(l * r),
                
                (OperatorBinaryTag.Div, double l, double r)
                    => System.Create(l / r),
                
                (OperatorBinaryTag.Mod, double l, double r)
                    => System.Create(l % r),
                
                (OperatorBinaryTag.Lt, double l, double r)
                    => System.Create(l < r),
                
                (OperatorBinaryTag.LtEq, double l, double r)
                    => System.Create(l <= r),
                
                (OperatorBinaryTag.Gt, double l, double r)
                    => System.Create(l > r),
                
                (OperatorBinaryTag.GtEq, double l, double r)
                    => System.Create(l >= r),
                _
                    => throw new RuntimeErrorException(
                            op.Position,
                            $"Operator {op.Tag} cannot be applied to values " +
                            $"<{left}> and <{right}>"),
            };
        }

        public Obj Visit(OperatorUnary op)
        {
            var obj = Evaluate(op.Expression);

            if (op.Op == OperatorUnaryTag.Not)
                return System.Create(!IsTruthy(obj));

            return (op.Op, obj.Value) switch
            {
                (OperatorUnaryTag.Neg, double n)
                    => System.Create(-n),
                _
                    => throw new RuntimeErrorException(
                            op.Position,
                            $"Operator {op.Op} cannot be applied to values `{obj}`"),
            };
        }

        public Obj Visit(Refinement rfnt)
        {
            var obj = Evaluate(rfnt.Target);
            var key = System.Create(rfnt.Name);
            return LookupProperty(obj, key);
        }

        public Obj Visit(Self self)
        {
            if (Self != null)
                return Self;
            else
                throw new RuntimeErrorException(
                    self.Position,
                    "self is not defined in this context");
        }

        public Obj Visit(Let let)
        {
            var result = Evaluate(let.InitialValue);
            if (!Environment.Create(let.Name.Name, result))
                throw new RuntimeErrorException(
                    let.Name.Position,
                    $"Identifier <{let.Name.Name}> is already bound");
            return result;
        }

        public Obj Visit(While @while)
        {
            while (IsTruthy(Evaluate(@while.Guard)))
                Evaluate(@while.Body);
            return System.Null;
        }

        public Obj Visit(For @for)
        {
            Callable? LookupMethod(Obj obj, string methodName)
            {
                var key = System.Create(methodName);
                var value = obj.LookupProperty(key);
                return value?.Value as Callable;
            }

            var iterable = Evaluate(@for.Iterable);

            Callable? getIterator = LookupMethod(iterable, "getIterator");
            if (getIterator == null)
                throw new RuntimeErrorException(
                    @for.Position,
                    "Object in `for` loop does not follow the iterable interface.");

            var emptyArgs = new Obj[0];                
            var itr = getIterator(this, iterable, emptyArgs);
            var next = LookupMethod(itr, "next");
            var current = LookupMethod(itr, "current");
            if (next == null || current == null)
                throw new RuntimeErrorException(
                    @for.Position,
                    "Object returned by `getIterator` does follow the iterable interface.");

            while (IsTruthy(next(this, itr, emptyArgs)))
            {
                var interpreter = Push();
                interpreter.Environment.Create(
                    @for.Variable.Name,
                    current(interpreter, itr, emptyArgs));

                interpreter.Evaluate(@for.Body);
            }

            return System.Null;
        }

        public Obj Visit(Write write)
        {
            foreach (var e in write.Arguments)
                Console.Write(Evaluate(e));
            return System.Null;
        }
    }
}