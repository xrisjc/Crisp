using System;
using System.Linq;
using Crisp.Ast;

namespace Crisp.Runtime
{
    class Interpreter : IExpressionVisitor<CrispObject>
    {
        public System System { get; }
        public Environment Globals { get; }
        public Environment Environment { get; }
        public CrispObject? Self { get; }

        public Interpreter(
            System system,
            Environment globals,
            Environment environment,
            CrispObject? self)
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

        Interpreter PushWith(CrispObject self)
            => new Interpreter(System, Globals, new Environment(Environment), self);

        bool IsTruthy(CrispObject obj)
            => obj switch
               {
                   ObjectBool x => x.Value,
                   ObjectNull _ => false,
                   _ => true,
               };

        CrispObject LookupProperty(CrispObject obj, CrispObject key)
            => obj.LookupProperty(key) ?? System.Null;

        public CrispObject Evaluate(IExpression expression)
            => expression.Accept(this);

        public CrispObject Visit(AssignmentIdentifier ai)
        {
            var result = Evaluate(ai.Value);
            if (!Environment.Set(ai.Target.Name, result))
                throw new RuntimeErrorException(
                    ai.Target.Position,
                    $"Cannot assign, <{ai.Target.Name}> is unbound");
            return result;
        }

        public CrispObject Visit(AssignmentIndex ai)
        {
            var target = Evaluate(ai.Index.Target);
            var key = Evaluate(ai.Index.Key);
            var value = Evaluate(ai.Value);
            target.SetProperty(key, value);
            return value;
        }

        public CrispObject Visit(AssignmentRefinement ar)
        {
            var target = Evaluate(ar.Refinement.Target);
            var key = System.Create(ar.Refinement.Name);
            var value = Evaluate(ar.Value);
            target.SetProperty(key, value);
            return value;
        }

        public CrispObject Visit(Block block)
        {
            var result = System.Null;
            {
                var interpreter = Push();
                foreach (var e in block.Body)
                    result = interpreter.Evaluate(e);
            }
            return result;
        }

        public CrispObject Visit(Call call)
        {
            CrispObject? self;
            CrispObject key;
            CrispObject value;
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

            if (value is ICallable fn)
            {
                var args = from arg in call.Arguments
                            select Evaluate(arg);
                return fn.Invoke(this, self, args.ToArray());
            }
            else
            {
                throw new RuntimeErrorException(
                    call.Position,
                    $"Cannot call non-callable object <{value}>.");
            }
        }

        public CrispObject Visit(If @if)
        {
            if (IsTruthy(Evaluate(@if.Condition)))
                return Push().Evaluate(@if.Consequence);
            else
                return Push().Evaluate(@if.Alternative);
        }

        public CrispObject Visit(Ast.Index index)
        {
            var obj = Evaluate(index.Target);
            var key = Evaluate(index.Key);
            return LookupProperty(obj, key);
        }

        public CrispObject Visit(Function function)
            => System.Create(function, Environment);

        public CrispObject Visit(Identifier identifier)
            => Environment.Get(identifier.Name) ??
                    throw new RuntimeErrorException(
                        identifier.Position,
                        $"Identifier <{identifier.Name}> unbound");

        public CrispObject Visit(LiteralBool literalBool)
            => System.Create(literalBool.Value);

        public CrispObject Visit(LiteralNull ln)
            => System.Null;

        public CrispObject Visit(LiteralNumber number)
            => System.Create(number.Value);

        public CrispObject Visit(LiteralString literalString)
            => System.Create(literalString.Value);

        public CrispObject Visit(LiteralList literalList)
        {
            var items = from expr in literalList.Items
                        select Evaluate(expr);
            return System.Create(items.ToList());
        }

        public CrispObject Visit(OperatorBinary op)
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
            return op.Tag switch
            {
                OperatorBinaryTag.Add when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value + r.Value),
                
                OperatorBinaryTag.Sub when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value - r.Value),
                
                OperatorBinaryTag.Mul when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value * r.Value),
                
                OperatorBinaryTag.Div when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value / r.Value),
                
                OperatorBinaryTag.Mod when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value % r.Value),
                
                OperatorBinaryTag.Lt when left is ObjectNumber l && 
                                            right is ObjectNumber r
                    => System.Create(l.Value < r.Value),
                OperatorBinaryTag.LtEq when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value <= r.Value),
                OperatorBinaryTag.Gt when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value > r.Value),
                OperatorBinaryTag.GtEq when left is ObjectNumber l &&
                                            right is ObjectNumber r
                    => System.Create(l.Value >= r.Value),
                OperatorBinaryTag.Eq
                    => System.Create(left.Equals(right)),
                OperatorBinaryTag.Neq
                    => System.Create(!left.Equals(right)),
                OperatorBinaryTag.Is
                    => System.Create(left.Is(right)),
                _
                    => throw new RuntimeErrorException(
                            op.Position,
                            $"Operator {op.Tag} cannot be applied to values " +
                            $"<{left}> and <{right}>"),
            };
        }

        public CrispObject Visit(OperatorUnary op)
        {
            var left = Evaluate(op.Expression);
            return op.Op switch
            {
                OperatorUnaryTag.Not
                    => System.Create(!IsTruthy(left)),
                OperatorUnaryTag.Neg when left is ObjectNumber n
                    => System.Create(-n.Value),
                _
                    => throw new RuntimeErrorException(
                            op.Position,
                            $"Operator {op.Op} cannot be applied to values <{left}>"),
            };
        }

        public CrispObject Visit(Refinement rfnt)
        {
            var obj = Evaluate(rfnt.Target);
            var key = System.Create(rfnt.Name);
            return LookupProperty(obj, key);
        }

        public CrispObject Visit(Self self)
        {
            if (Self != null)
                return Self;
            else
                throw new RuntimeErrorException(
                    self.Position,
                    "self is not defined in this context");
        }

        public CrispObject Visit(Var var)
        {
            var result = Evaluate(var.InitialValue);
            if (!Environment.Create(var.Name.Name, result))
                throw new RuntimeErrorException(
                    var.Name.Position,
                    $"Identifier <{var.Name.Name}> is already bound");
            return result;
        }

        public CrispObject Visit(While @while)
        {
            while (IsTruthy(Evaluate(@while.Guard)))
                Push().Evaluate(@while.Body);
            return System.Null;
        }

        public CrispObject Visit(For @for)
        {
            // Create new scope for the for loop variable.
            var interpreter = Push();

            var iterable = Evaluate(@for.Iterable);

            // Test if it has a getIterator function.
            var getIterator = iterable.LookupProperty(
                System.Create("getIterator"));

            // Is getIterator a callable?
            if (getIterator is ICallable getIteratorCallable)
            {
                var emptyArgs = new CrispObject[0];
                
                // Get the iterator.
                var itr = getIteratorCallable.Invoke(
                    interpreter, iterable, emptyArgs);

                // Getting the next and current functions on the
                // iterator object.
                var nextKey = System.Create("next");
                var currentKey = System.Create("current");
                var next = itr.LookupProperty(nextKey);
                var current = itr.LookupProperty(currentKey);

                // Test if next and current are callable objects.
                if (next is ICallable nextCallable &&
                    current is ICallable currentCallable)
                {
                    // Create our loop variable in the inner scope
                    // environment.
                    interpreter.Environment.Create(
                        @for.Variable.Name,
                        System.Null);

                    // Actually do our loop.
                    while (true)
                    {
                        // Move to next item in iterator.
                        var hasMore = 
                            nextCallable.Invoke(
                                interpreter, itr, emptyArgs);

                        // Is the iterator done yet?
                        if (!IsTruthy(hasMore)) break;

                        // We're not done, so update the loop
                        // variable.

                        var currentValue =
                            currentCallable.Invoke(
                                interpreter, itr, emptyArgs);

                        interpreter.Environment.Set(
                            @for.Variable.Name,
                            currentValue);

                        // Now evaluate the loop body.  Each loop
                        // body is also an inner scope.
                        var bodyInterpreter = interpreter.Push();
                        bodyInterpreter.Evaluate(@for.Body);
                    }
                }
                else throw new RuntimeErrorException(
                    @for.Position,
                    "Object returned by `getIterator` does follow the iterable interface.");
            }
            else throw new RuntimeErrorException(
                @for.Position,
                "Object in `for` loop does not follow the iterable interface.");

            return System.Null;
        }

        public CrispObject Visit(Write write)
        {
            foreach (var e in write.Arguments)
                Console.Write(
                    Evaluate(e) switch
                    {
                        ObjectBool x => x.Value ? "true" : "false",
                        ObjectNumber x => x.Value.ToString(),
                        ObjectString x => x.Value,
                        CrispObject x => x.ToString(),
                    });
            return System.Null;
        }
    }
}