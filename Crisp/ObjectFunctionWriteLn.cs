using System;
using System.Collections.Generic;

namespace Crisp
{
    class ObjectFunctionWriteLn : ObjectFunction
    {
        public override IObject Call(List<IObject> arguments)
        {
            foreach (var a in arguments)
            {
                Console.Write(a);
            }
            Console.WriteLine();
            return ObjectNull.Instance;
        }
    }
}
