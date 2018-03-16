using System;

namespace Crisp.Eval
{
    class RuntimeErrorException : CrispException
    {
        public RuntimeErrorException(string message)
            : base(message)
        {
        }

        public override string FormattedMessage() =>
            $"Runtime Error: {Message}";
    }
}
