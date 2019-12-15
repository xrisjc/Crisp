using System;
using System.Linq;
using Crisp.Ast;

namespace Crisp.Runtime
{
    class Interpreter
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

        Interpreter PushCall(CrispObject? self)
            => new Interpreter(System, Globals, new Environment(Globals), self);

        bool IsTruthy(CrispObject obj)
            => obj switch
               {
                   ObjectBool x => x.Value,
                   ObjectNull _ => false,
                   _ => true,
               };

        CrispObject LookupProperty(CrispObject obj, CrispObject key)
        {
            for (CrispObject? o = obj; o != null; o = o.Prototype)
                if (o.Properties.TryGetValue(key, out var value))
                    return value;
            return System.Null;
        }

        public CrispObject Evaluate(IExpression expression)
        {
            CrispObject result;
            switch (expression)
            {
                case AssignmentIdentifier ai:
                    result = Evaluate(ai.Value);
                    if (!Environment.Set(ai.Target.Name, result))
                        throw new RuntimeErrorException(
                            ai.Target.Position,
                            $"Cannot assign, <{ai.Target.Name}> is unbound");
                    break;

                case AssignmentIndex ai:
                    {
                        var target = Evaluate(ai.Index.Target);
                        var key = Evaluate(ai.Index.Key);
                        var value = Evaluate(ai.Value);
                        target.Properties[key] = value;
                        result = value;
                    }
                    break;

                case AssignmentRefinement ar:
                    {
                        var target = Evaluate(ar.Refinement.Target);
                        var key = System.Create(ar.Refinement.Name);
                        var value = Evaluate(ar.Value);
                        target.Properties[key] = value;
                        result = value;
                    }
                    break;

                case Block block:
                    result = System.Null;
                    {
                        var interpreter = Push();
                        foreach (var e in block.Body)
                            result = interpreter.Evaluate(e);
                    }
                    break;

                case Call call:
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
                            result = fn.Invoke(PushCall(self), args.ToArray());
                        }
                        else
                        {
                            throw new RuntimeErrorException(
                                call.Position,
                                $"Cannot call non-callable object <{value}>.");
                        }
                    }
                    break;

                case If @if:
                    if (IsTruthy(Evaluate(@if.Condition)))
                        result = Push().Evaluate(@if.Consequence);
                    else
                        result = Push().Evaluate(@if.Alternative);
                    break;

                case Ast.Index index:
                    {
                        var obj = Evaluate(index.Target);
                        var key = Evaluate(index.Key);
                        result = LookupProperty(obj, key);
                    }
                    break;

                case Function function:
                    result = System.Create(function);
                    break;

                case Identifier identifier:
                    result = Environment.Get(identifier.Name) ??
                                throw new RuntimeErrorException(
                                    identifier.Position,
                                    $"Identifier <{identifier.Name}> unbound");
                    break;

                case LiteralBool literalBool:
                    result = System.Create(literalBool.Value);
                    break;

                case LiteralNull _:
                    result = System.Null;
                    break;

                case LiteralNumber number:
                    result = System.Create(number.Value);
                    break;

                case LiteralObject literalObject:
                    result = System.Create();
                    foreach (var (key, value) in literalObject.Properties)
                        result.Properties[System.Create(key)] = Evaluate(value);
                    break;

                case LiteralString literalString:
                    result = System.Create(literalString.Value);
                    break;

                case OperatorBinary op when op.Tag == OperatorBinaryTag.And:
                    result = System.Create(
                                IsTruthy(Evaluate(op.Left)) &&
                                IsTruthy(Evaluate(op.Right)));
                    break;

                case OperatorBinary op when op.Tag == OperatorBinaryTag.Or:
                    result = System.Create(
                                IsTruthy(Evaluate(op.Left)) ||
                                IsTruthy(Evaluate(op.Right)));
                    break;

                case OperatorBinary op:
                    var left = Evaluate(op.Left);
                    var right = Evaluate(op.Right);
                    result = op.Tag switch
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
                        _
                            => throw new RuntimeErrorException(
                                   op.Position,
                                   $"Operator {op.Tag} cannot be applied to values " +
                                   $"<{left}> and <{right}>"),
                    };
                    break;

                case OperatorUnary op:
                    left = Evaluate(op.Expression);
                    result = op.Op switch
                    {
                        OperatorUnaryTag.Beget
                            => System.Beget(left),
                        OperatorUnaryTag.Not
                            => System.Create(!IsTruthy(left)),
                        OperatorUnaryTag.Neg when left is ObjectNumber n
                            => System.Create(-n.Value),
                        _
                            => throw new RuntimeErrorException(
                                   op.Position,
                                   $"Operator {op.Op} cannot be applied to values <{left}>"),
                    };
                    break;

                case Refinement rfnt:
                    {
                        var obj = Evaluate(rfnt.Target);
                        var key = System.Create(rfnt.Name);
                        result = LookupProperty(obj, key);
                    }
                    break;

                case Self self:
                    if (Self != null)
                        result = Self;
                    else
                        throw new RuntimeErrorException(
                            self.Position,
                            "self is not defined in this context");
                    break;

                case Var var:
                    result = Evaluate(var.InitialValue);
                    if (!Environment.Create(var.Name.Name, result))
                        throw new RuntimeErrorException(
                            var.Name.Position,
                            $"Identifier <{var.Name.Name}> is already bound");
                    break;

                case While @while:
                    while (IsTruthy(Evaluate(@while.Guard)))
                        Push().Evaluate(@while.Body);
                    result = System.Null;
                    break;

                case Write write:
                    foreach (var e in write.Arguments)
                        Console.Write(
                            Evaluate(e) switch
                            {
                                ObjectBool x => x.Value ? "true" : "false",
                                ObjectNumber x => x.Value.ToString(),
                                ObjectString x => x.Value,
                                CrispObject x => x.ToString(),
                            });
                    result = System.Null;
                    break;

                default:
                    throw new NotImplementedException(
                        $"Unimplemented {expression.GetType()}");
            }
            return result;
        }
    }
}