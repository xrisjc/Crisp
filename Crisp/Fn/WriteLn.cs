using Crisp.Eval;
using System;
using System.Collections.Generic;

namespace Crisp.Fn
{
    class WriteLn : IObj, IFn
    {
        public IType Type => TypeFn.Instance;

        public int? Arity => null;

        public IObj Call(List<IObj> args)
        {
            var line = string.Join("", args);
            Console.WriteLine(line);
            return ObjNull.Instance;
        }
    }
}
