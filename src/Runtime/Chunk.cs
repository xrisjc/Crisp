using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
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

        public void Emit(Op code)
        {
            Emit((int)code);
        }

        public void Emit(Op code, int a)
        {
            Emit(code);
            Emit(a);
        }

        public void Emit(Op code, CrispObject obj)
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
                i += DissassembleCode(i).Count();
        }

        public Op DissassembleCode(int i)
        {
            int offset = -1;
            var code = (Op)Code[i];
            Console.Write("{0:D8} {1}", i, code);
            switch (code)
            {
                case Op.Const:
                    offset = Code[i + 1];
                    Console.WriteLine(
                        " {0:D4} ({1})",
                        offset,
                        Constants[offset]);
                    break;
                case Op.Fn:
                    offset = Code[i + 1];
                    Console.WriteLine(" @{0:D8}", offset);
                    break;
                case Op.Jump:
                case Op.JumpTruthy:
                case Op.JumpFalsy:
                    offset = Code[i + 1];
                    Console.WriteLine(" {0:D8}", offset);
                    break;
                case Op.Return:
                case Op.Halt:
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
}