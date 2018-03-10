using System;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjFnWriteLn : IObj, IFn
    {
        public IObj Call(List<IObj> args)
        {
            var line = string.Join("", args);
            Console.WriteLine(line);
            return ObjNull.Instance;
        }

        public string Print()
        {
            return "<fn>";
        }
    }
}
