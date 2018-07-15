using System.Collections.Generic;

namespace Crisp.Runtime
{
    interface IEntity
    {
        bool GetAttribute(string name, out object value);
        bool SetAttribute(string name, object value);
        bool SendMessage(string name, List<object> arguments, out object value);
    }
}
