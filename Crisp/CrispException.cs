using System;

namespace Crisp
{
    abstract class CrispException : Exception
    {
        public CrispException(string message)
            : base(message)
        {
        }

        public abstract string FormattedMessage();
    }
}
