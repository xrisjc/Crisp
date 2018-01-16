using System.Collections.Generic;

namespace Crisp
{
    abstract class ObjectFunction : IObject
    {
        public abstract IObject Call(List<IObject> arguments);
    }
}
