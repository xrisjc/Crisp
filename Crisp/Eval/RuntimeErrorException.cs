using System;

namespace Crisp.Eval
{
    class RuntimeErrorException : Exception
    {
        public RuntimeErrorException(string message)
            : base(message)
        {
        }
    }
}
