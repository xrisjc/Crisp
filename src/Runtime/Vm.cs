using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Vm
    {
        static bool IsTruthy(CrispObject obj)
            => obj switch
            {
                ObjectBool x => x.Value,
                ObjectNull _ => false,
                _ => true,
            };

        static CrispObject? LookupProperty(CrispObject obj, CrispObject key)
        {
            for (CrispObject? o = obj; o != null; o = o.Prototype)
                if (o.Properties.TryGetValue(key, out var value))
                    return value;
            return null;
        }

        static void Print<T>(Stack<T> stack)
        {
            foreach (var x in stack)
                Console.Write("[{0}]", x?.ToString());
            Console.WriteLine();
        }

        public static void Run(Library system, Chunk chunk)
        {
            var globals = new Environment2(null);
            var frame = new Frame(0, globals, null);

            var callStack = new Stack<Frame>();
            var stack = new Stack<CrispObject>();


            var halt = false;
            while (!halt)
            {
                CrispObject L, R, K, V;
                int offset;

                //chunk.DissassembleCode(frame.Offset);

                var code = (Op)chunk.Code[frame.Offset++];
                switch (code)
                {
                    case Op.Add:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value + RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.Beget:
                        L = stack.Pop();
                        V = system.Beget(L);
                        stack.Push(V);
                        break;
                    
                    case Op.Call:
                    case Op.CallMthd:
                        if (stack.Pop() is ObjectFunction fn)
                        {
                            callStack.Push(frame);
                            var env = new Environment2(globals);
                            var self = (code == Op.CallMthd) ? stack.Pop() : null;
                            frame = new Frame(fn.Offset, env, self);
                        }
                        else
                            throw new RuntimeErrorException(
                                new Parsing.Position(-1, -1),
                                "cannot call a non-function");
                        break;
                    
                    case Op.Const:
                        V = chunk.Constants[chunk.Code[frame.Offset++]];
                        stack.Push(V);
                        break;

                    case Op.CreateVar:
                        V = stack.Pop();
                        K = stack.Pop();
                        frame.Environment.Create(K, V);
                        stack.Push(V);
                        break;
                    
                    case Op.Discard:
                        stack.Pop();
                        break;
                    
                    case Op.Div:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value / RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.Dup:
                        V = stack.Peek();
                        stack.Push(V);
                        break;
                    
                    case Op.EndBlock:
                        frame.EndBlock();
                        break;
                    
                    case Op.Eq:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = system.Create(L.Equals(R));
                        stack.Push(V);
                        break;
                    
                    case Op.False:
                        V = system.False;
                        stack.Push(V);
                        break;
                    
                    case Op.Fn:
                        offset = chunk.Code[frame.Offset++];
                        V = new ObjectFunction(system.PrototypeFunction, offset);
                        stack.Push(V);
                        break;
                    
                    case Op.GetProp:
                        K = stack.Pop();
                        L = stack.Pop();
                        V = LookupProperty(L, K) ?? system.Null;
                        stack.Push(V);
                        break;

                    case Op.GetVar:
                        K = stack.Pop();
                        V = frame.Environment.Get(K) ?? system.Null;
                        stack.Push(V);
                        break;
                    
                    case Op.Gt:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value > RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.GtEq:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value >= RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.Halt:
                        halt = true;
                        break;
                    
                    case Op.Jump:
                        offset = chunk.Code[frame.Offset++];
                        frame.Offset = offset;
                        break;
                    
                    case Op.JumpFalsy:
                        offset = chunk.Code[frame.Offset++];
                        L = stack.Pop();
                        if (!IsTruthy(L))
                            frame.Offset = offset;
                        break;
                    
                    case Op.JumpTruthy:
                        offset = chunk.Code[frame.Offset++];
                        L = stack.Pop();
                        if (IsTruthy(L))
                            frame.Offset = offset;
                        break;
                    
                    case Op.Lt:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value < RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.LtEq:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value <= RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.Mod:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value % RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.Mul:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value * RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.Neg:
                        L = stack.Pop();
                        V = L switch
                        {
                            ObjectNumber LN => system.Create(-LN.Value),
                            _ => throw new RuntimeErrorException(
                                     new Parsing.Position(-1, -1),
                                     "Number error"),
                        };
                        stack.Push(V);
                        break;
                    
                    case Op.New:
                        V = system.Create();
                        stack.Push(V);
                        break;
                    
                    case Op.Not:
                        L = stack.Pop();
                        V = system.Create(!IsTruthy(L));
                        stack.Push(V);
                        break;
                    
                    case Op.NotEq:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = system.Create(!L.Equals(R));
                        stack.Push(V);
                        break;
                    
                    case Op.Null:
                        V = system.Null;
                        stack.Push(V);
                        break;
                    
                    case Op.Return:
                        frame = callStack.Pop();
                        break;
                    
                    case Op.Self:
                        V = frame.Self ?? throw new RuntimeErrorException(
                                              new Parsing.Position(-1, -1),
                                              "self not defined");
                        stack.Push(V);
                        break;
                    
                    case Op.SetProp:
                        R = stack.Pop();
                        K = stack.Pop();
                        L = stack.Pop();
                        L.Properties[K] = R;
                        V = R;
                        stack.Push(V);
                        break;

                    case Op.SetVar:
                        V = stack.Pop();
                        K = stack.Pop();
                        frame.Environment.Set(K, V);
                        stack.Push(V);
                        break;
                    
                    case Op.StartBlock:
                        frame.StartBlock();
                        break;
                    
                    case Op.Sub:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = (L, R) switch
                        {
                            (ObjectNumber LN, ObjectNumber RN)
                                => system.Create(LN.Value - RN.Value),
                            _
                                => throw new RuntimeErrorException(
                                       new Parsing.Position(-1, -1),
                                       "Number error"),
                        };
                        stack.Push(V);
                        break;

                    case Op.Switch:
                        R = stack.Pop();
                        L = stack.Pop();
                        stack.Push(R);
                        stack.Push(L);
                        break;
                    
                    case Op.True:
                        V = system.True;
                        stack.Push(V);
                        break;
                    
                    case Op.Truthy:
                        L = stack.Pop();
                        V = system.Create(IsTruthy(L));
                        stack.Push(V);
                        break;
                    
                    case Op.Write:
                        L = stack.Pop();
                        Console.Write(L);
                        break;

                    default:
                        throw new NotImplementedException(
                            $"OpCode.{code} is not implemented");
                }
            }
        }
    }
}