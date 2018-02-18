using System;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjFnWriteLn : IObjFn
    {
        public IObj Call(List<IObj> args)
        {
            var line = string.Join("", args);
            Console.WriteLine(line);
            return Obj.Null;
        }

        public string Print()
        {
            return "<fn>";
        }
    }
}
