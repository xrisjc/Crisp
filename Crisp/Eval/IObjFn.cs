using System.Collections.Generic;

namespace Crisp.Eval
{
    interface IObjFn : IObj
    {
        IObj Call(List<IObj> args);
    }
}
