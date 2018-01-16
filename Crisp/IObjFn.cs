using System.Collections.Generic;

namespace Crisp
{
    interface IObjFn : IObj
    {
        IObj Call(List<IObj> args);
    }
}
