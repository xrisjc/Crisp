using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Fn
{
    class Len : IObj, IFn
    {
        public int? Arity => 1;

        public IObj Call(List<IObj> args)
        {
            if (args[0] is ILen len)
            {
                return new ObjInt(len.Len);
            }
            else
            {
                throw new RuntimeErrorException(
                    $"<{args[0]}> not supported by len()");
            }
        }
    }
}
