using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Fn
{
    class Push : IObj, IFn
    {
        public int? Arity => 2;

        public IObj Call(List<IObj> args)
        {
            if (args[0] is ObjList list)
            {
                return list.Push(args[1]);
            }
            else
            {
                throw new RuntimeErrorException(
                    $"<{args[0]}> not supported by push()");
            }
        }
    }
}
