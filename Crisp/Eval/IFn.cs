using System.Collections.Generic;

namespace Crisp.Eval
{
    interface IFn
    {
        IObj Call(List<IObj> args);
    }
}
