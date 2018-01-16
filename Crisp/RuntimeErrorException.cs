using System;

namespace Crisp
{
    class RuntimeErrorException : Exception
    {
        public RuntimeErrorException(string message)
            : base(message)
        {
        }
    }
}
