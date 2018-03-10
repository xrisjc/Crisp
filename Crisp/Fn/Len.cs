using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Fn
{
    class Len : IObj, IFn
    {
        public IObj Call(List<IObj> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeErrorException(
                    "The len function requires 1 argument");
            }
            else if (args[0] is ILen len)
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
