using System;
using System.Collections.Generic;

namespace Crisp
{
    class ObjFnReadLn : IObjFn
    {
        public IObj Call(List<IObj> args)
        {
            // TODO: Handle args?
            var line = Console.ReadLine();
            return Obj.Create(line);
        }

        public string Print()
        {
            return "<fn>";
        }
    }
}
