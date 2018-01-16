using System;
using System.Collections.Generic;

namespace Crisp
{
    class ObjFnWriteLn : IObjFn
    {
        public IObj Call(List<IObj> args)
        {
            foreach (var a in args)
            {
                Console.Write(a);
            }
            Console.WriteLine();
            return Obj.Null;
        }
    }
}
