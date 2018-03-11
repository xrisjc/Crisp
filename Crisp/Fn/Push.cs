using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Fn
{
    class Push : IObj, IFn
    {
        public IObj Call(List<IObj> args)
        {
            if (args.Count != 2)
            {
                throw new RuntimeErrorException(
                    "The push() requires 2 arguments");
            }
            else if (args[0] is ObjList list)
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
