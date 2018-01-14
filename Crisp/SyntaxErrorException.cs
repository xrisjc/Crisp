using System;

namespace Crisp
{
    class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(string message)
            : base(message)
        {
        }
    }
}
