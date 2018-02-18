using System;

namespace Crisp.Parsing
{
    class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(string message)
            : base(message)
        {
        }
    }
}
