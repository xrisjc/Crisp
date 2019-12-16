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
                    Compile(ai.Value);
                    Chunk.Emit(OpCode.SetVar, System.Create(ai.Target));
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
                    Compile(c.Target);
                    Chunk.Emit(OpCode.Call);
                    Chunk.Emit(c.Arguments.Count);
                    break;

                case Function f:
                    foreach (var p in f.Parameters)
                        Chunk.Emit(OpCode.Const, System.Create(p.Name));
                    Chunk.Emit(OpCode.Fn);
                    Chunk.Emit(functions.Index(f));
                    Chunk.Emit(f.Parameters.Count);
                    break;

                case Identifier i:
                    Chunk.Emit(OpCode.GetVar, System.Create(i.Name));
                    break;

                case If i:
                    {
                        var lAlt = labels.New();
                        var lEnd = labels.New();
                        Compile(i.Condition);
                        Chunk.Emit(OpCode.JumpFalsy, lAlt);
                        Compile(i.Consequence);
                        Chunk.Emit(OpCode.Jump, lEnd);
                        labels.Set(lAlt, Chunk.NextOffset);
                        Compile(i.Alternative);
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
                    Compile(v.InitialValue);
                    Chunk.Emit(OpCode.CreateVar, System.Create(v.Name));
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
                    break;
            }
        }
        public void Compile(Program program)
        {
            functions.Add(program);
            foreach (var expr in program.Expressions)
            {
                Compile(expr);
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
                switch (code)
                {
                    case OpCode.Fn:
                        i += 3;
                        break;

                    case OpCode.Const:
                    case OpCode.CreateVar:
                    case OpCode.GetVar:
                    case OpCode.SetVar:
                    case OpCode.Call:
                    case OpCode.Jump:
                    case OpCode.JumpTruthy:
                    case OpCode.JumpFalsy:
                        i += 2;
                        break;

                    default:
                        i += 1;
                        break;
                }
            }
        }
    }

    class Vm
    {
        CrispObject self;
    }

    class Chunk
    {
        public List<int> Code { get; } = new List<int>();
        public List<CrispObject> Constants { get; } = new List<CrispObject>();
        public int NextOffset => Code.Count + 1;
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
            Constants.Add(obj);
            Emit(Constants.Count - 1);
        }

        public void Dissassemble()
        {
            int i = 0;
            while (i < Code.Count)
            {
                int offset;
                var code = (OpCode)Code[i++];
                Console.Write("{0:D8} {1}", i, code);
                switch (code)
                {
                    case OpCode.Const:
                    case OpCode.CreateVar:
                    case OpCode.GetVar:
                    case OpCode.SetVar:
                        offset = Code[i++];
                        Console.WriteLine(
                            " {0:D4} ({1})",
                            offset,
                            Constants[offset] switch
                            {
                                ObjectBool x => x.Value ? "true" : "false",
                                ObjectNumber x => x.Value.ToString(),
                                ObjectString x => x.Value,
                                CrispObject x => x.ToString(),
                            });
                        break;
                    case OpCode.Fn:
                        offset = Code[i++];
                        var nParams = Code[i++];
                        Console.WriteLine(" @{0:D8}, {1}", offset, nParams);
                        break;
                    case OpCode.Call:
                        Console.WriteLine(" {0}", Code[i++]);
                        break;
                    case OpCode.Jump:
                    case OpCode.JumpTruthy:
                    case OpCode.JumpFalsy:
                        Console.WriteLine(" {0:D8}", Code[i++]);
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
            }
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