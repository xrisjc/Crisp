﻿using System;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjFnReadLn : IObjFn
    {
        public IObj Call(List<IObj> args)
        {

            if (args.Count > 0)
            {
                var prompt = string.Join("", args);
                Console.Write(prompt);
            }
            var line = Console.ReadLine();
            return Obj.Create(line);
        }

        public string Print()
        {
            return "<fn>";
        }
    }
}