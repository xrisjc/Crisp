using System.Collections.Generic;

namespace Crisp.Eval
{
    interface IFn
    {
        int? Arity { get; }
        IObj Call(List<IObj> arguments);
    }
}
