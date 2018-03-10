using Crisp.Eval;
using System;
using System.Collections.Generic;

namespace Crisp.Fn
{
    class ReadLn : IObj, IFn
    {
        public IObj Call(List<IObj> args)
        {
            if (args.Count > 0)
            {
                var prompt = string.Join("", args);
                Console.Write(prompt);
            }
            var line = Console.ReadLine();
            return new ObjStr(line);
        }
    }
}
