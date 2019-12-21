using Crisp.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crisp.Runtime.Vm
{
    enum OpCode : int
    {
        Add,
        Beget,
        Call,
        CallMthd,
        Const,
        CreateVar,
        Discard,
        Div,
        Dup,
        EndBlock,
        Eq,
        False,
        Fn,
        GetProp,
        GetVar,
        Gt,
        GtEq,
        Halt,
        Jump,
        JumpFalsy,
        JumpTruthy,
        Lt,
        LtEq,
        Mod,
        Mul,
        Neg,
        New,
        Not,
        NotEq,
        Null,
        Return,
        Self,
        SetProp,
        SetVar,
        StartBlock,
        Sub,
        True,
        Truthy,
        Write,
    }

    static class OpCodeUtil
    {
        public static int OpSize(this OpCode code)
        {
            switch (code)
            {
                case OpCode.Fn:
                    return 3;

                case OpCode.Const:
                case OpCode.Call:
                case OpCode.CallMthd:
                case OpCode.Jump:
                case OpCode.JumpTruthy:
                case OpCode.JumpFalsy:
                    return 2;

                default:
                    return 1;
            }
        }
    }

    class Compiler
    {
        public System System { get; } = new System();
        
        Labels labels = new Labels();
        FunctionLookup functions = new FunctionLookup();

        public Chunk Chunk { get; } = new Chunk();

        void Compile(IExpression expr)
        {
            switch (expr)
            {
                case AssignmentIdentifier ai:
                    Chunk.Emit(OpCode.Const, System.Create(ai.Target.Name));
                    Compile(ai.Value);
                    Chunk.Emit(OpCode.SetVar);
                    break;

                case AssignmentIndex ai:
                    Compile(ai.Index.Target);
                    Compile(ai.Index.Key);
                    Compile(ai.Value);
                    Chunk.Emit(OpCode.SetProp);
                    break;

                case AssignmentRefinement ar:
                    Compile(ar.Refinement.Target);
                    Chunk.Emit(OpCode.Const, System.Create(ar.Refinement.Name));
                    Compile(ar.Value);
                    Chunk.Emit(OpCode.SetProp);
                    break;

                case Block b:
                    Chunk.Emit(OpCode.StartBlock);
                    if (b.Body.Count > 0)
                        for (int i = 0; i < b.Body.Count; i++)
                        {
                            Compile(b.Body[i]);
                            if (i < b.Body.Count - 1)
                                Chunk.Emit(OpCode.Discard);
                        }
                    else
                        Chunk.Emit(OpCode.Null);
                    Chunk.Emit(OpCode.EndBlock);
                    break;

                case Call c:
                    foreach (var e in c.Arguments)
                        Compile(e);
                    
                    switch (c.Target)
                    {
                        case Ast.Index index:
                            // self
                            Compile(index.Target);
                            // fn
                            Chunk.Emit(OpCode.Dup);
                            Compile(index.Key);
                            Chunk.Emit(OpCode.GetProp);
                            Chunk.Emit(OpCode.CallMthd, c.Arguments.Count);
                            break;
                        
                        case Refinement rfnt:
                            // self
                            Compile(rfnt.Target);
                            // fn
                            Chunk.Emit(OpCode.Dup);
                            Chunk.Emit(OpCode.Const, System.Create(rfnt.Name));
                            Chunk.Emit(OpCode.GetProp);
                            Chunk.Emit(OpCode.CallMthd, c.Arguments.Count);
                            break;
                        
                        default:
                            // fn
                            Compile(c.Target);
                            Chunk.Emit(OpCode.Call, c.Arguments.Count);
                            break;
                    }

                    break;

                case Function f:
                    foreach (var p in f.Parameters)
                        Chunk.Emit(OpCode.Const, System.Create(p.Name));
                    Chunk.Emit(OpCode.Fn);
                    Chunk.Emit(functions.Index(f));
                    Chunk.Emit(f.Parameters.Count);
                    break;

                case Identifier i:
                    Chunk.Emit(OpCode.Const, System.Create(i.Name));
                    Chunk.Emit(OpCode.GetVar);
                    break;

                case If i:
                    {
                        var lAlt = labels.New();
                        var lEnd = labels.New();
                        Compile(i.Condition);
                        Chunk.Emit(OpCode.JumpFalsy, lAlt);
                        Chunk.Emit(OpCode.StartBlock);
                        Compile(i.Consequence);
                        Chunk.Emit(OpCode.EndBlock);
                        Chunk.Emit(OpCode.Jump, lEnd);
                        labels.Set(lAlt, Chunk.NextOffset);
                        Chunk.Emit(OpCode.StartBlock);
                        Compile(i.Alternative);
                        Chunk.Emit(OpCode.EndBlock);
                        labels.Set(lEnd, Chunk.NextOffset);
                    }
                    break;

                case Ast.Index i:
                    Compile(i.Target);
                    Compile(i.Key);
                    Chunk.Emit(OpCode.GetProp);
                    break;

                case LiteralBool lb:
                    Chunk.Emit(lb.Value ? OpCode.True : OpCode.False);
                    break;

                case LiteralNull _:
                    Chunk.Emit(OpCode.Null);
                    break;

                case LiteralObject lo:
                    Chunk.Emit(OpCode.New);
                    foreach (var (n, e) in lo.Properties)
                    {
                        Chunk.Emit(OpCode.Dup);
                        Chunk.Emit(OpCode.Const, System.Create(n));
                        Compile(e);
                        Chunk.Emit(OpCode.SetProp);
                        Chunk.Emit(OpCode.Discard);
                    }
                    break;

                case LiteralNumber ln:
                    Chunk.Emit(OpCode.Const, System.Create(ln.Value));
                    break;

                case LiteralString ls:
                    Chunk.Emit(OpCode.Const, System.Create(ls.Value));
                    break;

                case OperatorBinary ob when ob.Tag == OperatorBinaryTag.And:
                    {
                        var lShortCiruit = labels.New();
                        Compile(ob.Left);
                        Chunk.Emit(OpCode.Truthy);
                        Chunk.Emit(OpCode.Dup);
                        Chunk.Emit(OpCode.JumpFalsy, lShortCiruit);
                        Chunk.Emit(OpCode.Discard);
                        Compile(ob.Right);
                        Chunk.Emit(OpCode.Truthy);
                        labels.Set(lShortCiruit, Chunk.NextOffset);
                    }
                    break;

                case OperatorBinary ob when ob.Tag == OperatorBinaryTag.Or:
                    {
                        var lShortCiruit = labels.New();
                        Compile(ob.Left);
                        Chunk.Emit(OpCode.Truthy);
                        Chunk.Emit(OpCode.Dup);
                        Chunk.Emit(OpCode.JumpTruthy, lShortCiruit);
                        Chunk.Emit(OpCode.Discard);
                        Compile(ob.Right);
                        Chunk.Emit(OpCode.Truthy);
                        labels.Set(lShortCiruit, Chunk.NextOffset);
                    }
                    break;

                case OperatorBinary ob:
                    Compile(ob.Left);
                    Compile(ob.Right);
                    switch (ob.Tag)
                    {
                        case OperatorBinaryTag.Add:
                            Chunk.Emit(OpCode.Add);
                            break;
                        case OperatorBinaryTag.Sub:
                            Chunk.Emit(OpCode.Sub);
                            break;
                        case OperatorBinaryTag.Mul:
                            Chunk.Emit(OpCode.Mul);
                            break;
                        case OperatorBinaryTag.Div:
                            Chunk.Emit(OpCode.Div);
                            break;
                        case OperatorBinaryTag.Mod:
                            Chunk.Emit(OpCode.Mod);
                            break;
                        case OperatorBinaryTag.Lt:
                            Chunk.Emit(OpCode.Lt);
                            break;
                        case OperatorBinaryTag.LtEq:
                            Chunk.Emit(OpCode.LtEq);
                            break;
                        case OperatorBinaryTag.Gt:
                            Chunk.Emit(OpCode.Gt);
                            break;
                        case OperatorBinaryTag.GtEq:
                            Chunk.Emit(OpCode.GtEq);
                            break;
                        case OperatorBinaryTag.Eq:
                            Chunk.Emit(OpCode.Eq);
                            break;
                        case OperatorBinaryTag.Neq:
                            Chunk.Emit(OpCode.NotEq);
                            break;
                    }
                    break;

                case OperatorUnary ou:
                    Compile(ou.Expression);
                    switch (ou.Op)
                    {
                        case OperatorUnaryTag.Beget:
                            Chunk.Emit(OpCode.Beget);
                            break;
                        case OperatorUnaryTag.Neg:
                            Chunk.Emit(OpCode.Neg);
                            break;
                        case OperatorUnaryTag.Not:
                            Chunk.Emit(OpCode.Not);
                            break;
                    }
                    break;

                case Refinement r:
                    Compile(r.Target);
                    Chunk.Emit(OpCode.Const, System.Create(r.Name));
                    Chunk.Emit(OpCode.GetProp);
                    break;

                case Self _:
                    Chunk.Emit(OpCode.Self);
                    break;

                case Var v:
                    Chunk.Emit(OpCode.Const, System.Create(v.Name));
                    Compile(v.InitialValue);
                    Chunk.Emit(OpCode.CreateVar);
                    break;

                case While w:
                    {
                        var lStart = labels.New();
                        var lEnd = labels.New();
                        labels.Set(lStart, Chunk.NextOffset);
                        Compile(w.Guard);
                        Chunk.Emit(OpCode.JumpFalsy, lEnd);
                        Chunk.Emit(OpCode.StartBlock);
                        Compile(w.Body);
                        Chunk.Emit(OpCode.EndBlock);
                        Chunk.Emit(OpCode.Discard);
                        Chunk.Emit(OpCode.Jump, lStart);
                        labels.Set(lEnd, Chunk.NextOffset);
                        Chunk.Emit(OpCode.Null);
                    }
                    break;

                case Write w:
                    foreach (var e in w.Arguments)
                    {
                        Compile(e);
                        Chunk.Emit(OpCode.Write);
                    }
                    Chunk.Emit(OpCode.Null);
                    break;
            }
        }
        public void Compile(Program program)
        {
            functions.Add(program);
            foreach (var expr in program.Expressions)
            {
                Compile(expr);
                Chunk.Emit(OpCode.Discard);
            }
            Chunk.Emit(OpCode.Halt);
            foreach (var fn in functions.Functions)
            {
                functions.SetOffset(fn, Chunk.NextOffset);
                Compile(fn.Body);
                Chunk.Emit(OpCode.Return);
            }
            
            // Ajust jumps and function creation to where they actually are in
            // the chunk.
            int i = 0;
            while (i < Chunk.Code.Count)
            {
                var code = (OpCode)Chunk.Code[i];
                switch (code)
                {
                    case OpCode.Fn:
                        Chunk.Code[i + 1] = functions.Offset(Chunk.Code[i + 1]);
                        break;
                    case OpCode.Jump:
                    case OpCode.JumpFalsy:
                    case OpCode.JumpTruthy:
                        Chunk.Code[i + 1] = labels.Offset(Chunk.Code[i + 1]);
                        break;
                }
                i += code.OpSize();
            }
        }
    }

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
                Console.Write("[{0}]", x.ToString());
            Console.WriteLine();
        }

        public static void Run(System system, Chunk chunk)
        {
            var globals = new Environment2(null);
            var frame = new Frame(0, globals, null);

            var callStack = new Stack<Frame>();
            var stack = new Stack<CrispObject>();


            var halt = false;
            while (!halt)
            {
                CrispObject L, R, K, V;
                int offset, count;

                //chunk.DissassembleCode(frame.Offset);

                var code = (OpCode)chunk.Code[frame.Offset++];
                switch (code)
                {
                    case OpCode.Add:
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
                    
                    case OpCode.Beget:
                        L = stack.Pop();
                        V = system.Beget(L);
                        stack.Push(V);
                        break;
                    
                    case OpCode.Call:
                    case OpCode.CallMthd:
                        count = chunk.Code[frame.Offset++];
                        if (stack.Pop() is ObjectFunction2 fn)
                        {
                            var self = (code == OpCode.CallMthd) ? stack.Pop() : null;

                            var args = new CrispObject[count];
                            for (int i = count - 1; i >= 0; i--)
                                args[i] = stack.Pop();

                            var pars = fn.Parameters;
                            var env = new Environment2(globals);
                            for (int i = 0; i < pars.Length; i++)
                                env.Create(pars[i], i < args.Length ? args[i] : system.Null);

                            callStack.Push(frame);
                            frame = new Frame(fn.Offset, env, self);
                        }
                        else
                            throw new RuntimeErrorException(
                                new Parsing.Position(-1, -1),
                                "cannot call a non-function");
                        break;
                    
                    case OpCode.Const:
                        V = chunk.Constants[chunk.Code[frame.Offset++]];
                        stack.Push(V);
                        break;

                    case OpCode.CreateVar:
                        V = stack.Pop();
                        K = stack.Pop();
                        frame.Environment.Create(K, V);
                        stack.Push(V);
                        break;
                    
                    case OpCode.Discard:
                        stack.Pop();
                        break;
                    
                    case OpCode.Div:
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
                    
                    case OpCode.Dup:
                        V = stack.Peek();
                        stack.Push(V);
                        break;
                    
                    case OpCode.EndBlock:
                        frame.EndBlock();
                        break;
                    
                    case OpCode.Eq:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = system.Create(L.Equals(R));
                        stack.Push(V);
                        break;
                    
                    case OpCode.False:
                        V = system.False;
                        stack.Push(V);
                        break;
                    
                    case OpCode.Fn:
                        {
                            offset = chunk.Code[frame.Offset++];
                            count = chunk.Code[frame.Offset++];
                            var pars = new CrispObject[count];
                            for (int i = count - 1; i >= 0; i--)
                                pars[i] = stack.Pop();
                            V = new ObjectFunction2(system.PrototypeFunction, offset, pars);
                            stack.Push(V);
                        }
                        break;
                    
                    case OpCode.GetProp:
                        K = stack.Pop();
                        L = stack.Pop();
                        V = LookupProperty(L, K) ?? system.Null;
                        stack.Push(V);
                        break;

                    case OpCode.GetVar:
                        K = stack.Pop();
                        V = frame.Environment.Get(K) ?? system.Null;
                        stack.Push(V);
                        break;
                    
                    case OpCode.Gt:
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
                    
                    case OpCode.GtEq:
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
                    
                    case OpCode.Halt:
                        halt = true;
                        break;
                    
                    case OpCode.Jump:
                        offset = chunk.Code[frame.Offset++];
                        frame.Offset = offset;
                        break;
                    
                    case OpCode.JumpFalsy:
                        offset = chunk.Code[frame.Offset++];
                        L = stack.Pop();
                        if (!IsTruthy(L))
                            frame.Offset = offset;
                        break;
                    
                    case OpCode.JumpTruthy:
                        offset = chunk.Code[frame.Offset++];
                        L = stack.Pop();
                        if (IsTruthy(L))
                            frame.Offset = offset;
                        break;
                    
                    case OpCode.Lt:
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
                    
                    case OpCode.LtEq:
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
                    
                    case OpCode.Mod:
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
                    
                    case OpCode.Mul:
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
                    
                    case OpCode.Neg:
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
                    
                    case OpCode.New:
                        V = system.Create();
                        stack.Push(V);
                        break;
                    
                    case OpCode.Not:
                        L = stack.Pop();
                        V = system.Create(!IsTruthy(L));
                        stack.Push(V);
                        break;
                    
                    case OpCode.NotEq:
                        R = stack.Pop();
                        L = stack.Pop();
                        V = system.Create(!L.Equals(R));
                        stack.Push(V);
                        break;
                    
                    case OpCode.Null:
                        V = system.Null;
                        stack.Push(V);
                        break;
                    
                    case OpCode.Return:
                        frame = callStack.Pop();
                        break;
                    
                    case OpCode.Self:
                        V = frame.Self ?? throw new RuntimeErrorException(
                                              new Parsing.Position(-1, -1),
                                              "self not defined");
                        stack.Push(V);
                        break;
                    
                    case OpCode.SetProp:
                        R = stack.Pop();
                        K = stack.Pop();
                        L = stack.Pop();
                        L.Properties[K] = R;
                        V = R;
                        stack.Push(V);
                        break;

                    case OpCode.SetVar:
                        V = stack.Pop();
                        K = stack.Pop();
                        frame.Environment.Set(K, V);
                        stack.Push(V);
                        break;
                    
                    case OpCode.StartBlock:
                        frame.StartBlock();
                        break;
                    
                    case OpCode.Sub:
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
                    
                    case OpCode.True:
                        V = system.True;
                        stack.Push(V);
                        break;
                    
                    case OpCode.Truthy:
                        L = stack.Pop();
                        V = system.Create(IsTruthy(L));
                        stack.Push(V);
                        break;
                    
                    case OpCode.Write:
                        L = stack.Pop();
                        Console.Write(L);
                        break;

                    default:
                        throw new NotImplementedException(
                            $"OpCode.{code} is not implemented");
                }

                //Print(stack);`
            }
        }
    }

    class ObjectFunction2 : CrispObject
    {
        public int Offset { get; }
        public CrispObject[] Parameters { get; }

        public ObjectFunction2(CrispObject prototype, int offset, CrispObject[] parameters)
            : base(prototype)
        {
            Offset = offset;
            Parameters = parameters;
        }
        public override string ToString() => string.Format("<fn@{0:D8}>", Offset);
    }

    class Environment2
    {
        public Environment2? Outer { get; }
        Dictionary<CrispObject, CrispObject> values =
            new Dictionary<CrispObject, CrispObject>();

        public Environment2(Environment2? outer = null)
        {
            Outer = outer;
        }

        public CrispObject? Get(CrispObject key)
        {
            for (Environment2? e = this; e != null; e = e.Outer)
                if (e.values.TryGetValue(key, out var value))
                    return value;
            return null;
        }

        public void Set(CrispObject key, CrispObject value)
        {
            for (Environment2? e = this; e != null; e = e.Outer)
                if (e.values.ContainsKey(key))
                {
                    e.values[key] = value;
                    break;
                }
        }

        public void Create(CrispObject key, CrispObject value)
            => values.Add(key, value);
    }

    class Frame
    {
        public int Offset { get; set; }
        public CrispObject? Self { get; }
        public Environment2 Environment { get; private set; }

        public Frame(
            int offset,
            Environment2 environment,
            CrispObject? self)
        {
            Offset = offset;
            Environment = environment;
            Self = self;
        }

        public void StartBlock()
        {
            Environment = new Environment2(Environment);
        }

        public void EndBlock()
        {
            Environment = Environment.Outer;
        }
    }

    class Chunk
    {
        Dictionary<CrispObject, int> constantIndices = new Dictionary<CrispObject, int>();
        public List<int> Code { get; } = new List<int>();
        public List<CrispObject> Constants { get; } = new List<CrispObject>();
        public int NextOffset => Code.Count;
        public void Emit(int code)
        {
            Code.Add(code);
        }

        public void Emit(OpCode code)
        {
            Emit((int)code);
        }

        public void Emit(OpCode code, int a)
        {
            Emit(code);
            Emit(a);
        }

        public void Emit(OpCode code, CrispObject obj)
        {
            Emit(code);
            if (constantIndices.TryGetValue(obj, out var index))
            {
                Emit(index);
            }
            else
            {
                Constants.Add(obj);
                index = Constants.Count - 1;
                constantIndices.Add(obj, index);
                Emit(index);
            }
        }

        public void Dissassemble()
        {
            int i = 0;
            while (i < Code.Count)
                i += DissassembleCode(i).OpSize();
        }

        public OpCode DissassembleCode(int i)
        {
            int offset = -1, count = -1;
            var code = (OpCode)Code[i];
            Console.Write("{0:D8} {1}", i, code);
            switch (code)
            {
                case OpCode.Const:
                    offset = Code[i + 1];
                    Console.WriteLine(
                        " {0:D4} ({1})",
                        offset,
                        Constants[offset]);
                    break;
                case OpCode.Fn:
                    offset = Code[i + 1];
                    count = Code[i + 2];
                    Console.WriteLine(" @{0:D8}, {1}", offset, count);
                    break;
                case OpCode.Call:
                    offset = Code[i + 1];
                    Console.WriteLine(" {0}", offset);
                    break;
                case OpCode.Jump:
                case OpCode.JumpTruthy:
                case OpCode.JumpFalsy:
                    offset = Code[i + 1];
                    Console.WriteLine(" {0:D8}", offset);
                    break;
                case OpCode.Return:
                case OpCode.Halt:
                    Console.WriteLine();
                    Console.WriteLine("****************************************");
                    break;
                default:
                    Console.WriteLine();
                    break;
            }
            return code;
        }
    }

    class FunctionLookup
    {
        List<int> offsets = new List<int>();
        Dictionary<Function, int> indices = new Dictionary<Function, int>();
        
        public List<Function> Functions { get; } = new List<Function>();
        
        public int Index(Function function) => indices[function];
        
        public int Offset(int index) => offsets[index];
        
        public void SetOffset(Function function, int offset)
        {
            offsets[Index(function)] = offset;
        }
        
        void Add(Function function)
        {
            if (!indices.TryGetValue(function, out int index))
            {
                Functions.Add(function);
                offsets.Add(-1);
                index = Functions.Count - 1;
                indices[function] = index;
            }
        }
        
        void Add(IExpression expr)
        {
            switch (expr)
            {
                case AssignmentIdentifier ai:
                    Add(ai.Value);
                    break;

                case AssignmentIndex ai:
                    Add(ai.Index);
                    Add(ai.Value);
                    break;

                case AssignmentRefinement ar:
                    Add(ar.Refinement);
                    Add(ar.Value);
                    break;

                case Block b:
                    foreach (var e in b.Body)
                        Add(e);
                    break;

                case Call c:
                    Add(c.Target);
                    foreach (var e in c.Arguments)
                        Add(e);
                    break;

                case Function f:
                    Add(f);
                    Add(f.Body);
                    break;

                case If i:
                    Add(i.Condition);
                    Add(i.Consequence);
                    Add(i.Alternative);
                    break;

                case Ast.Index i:
                    Add(i.Target);
                    Add(i.Key);
                    break;

                case LiteralObject lo:
                    foreach (var (_, e) in lo.Properties)
                        Add(e);
                    break;

                case LiteralString _:
                    break;

                case OperatorBinary ob:
                    Add(ob.Left);
                    Add(ob.Right);
                    break;

                case OperatorUnary ou:
                    Add(ou.Expression);
                    break;

                case Refinement r:
                    Add(r.Target);
                    break;

                case Var v:
                    Add(v.InitialValue);
                    break;

                case While w:
                    Add(w.Guard);
                    Add(w.Body);
                    break;

                case Write w:
                    foreach (var e in w.Arguments)
                        Add(e);
                    break;
            }
        }
        
        public void Add(Program program)
        {
            foreach (var e in program.Expressions)
                Add(e);
        }
    }

    class Labels
    {
        List<int> labels = new List<int>();
        public int New()
        {
            labels.Add(-1);
            return labels.Count - 1;
        }
        public void Set(int label, int offset)
        {
            labels[label] = offset;
        }
        public int Offset(int label)
        {
            return labels[label];
        }
    }
}