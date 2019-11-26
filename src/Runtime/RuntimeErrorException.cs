using System;
using Crisp.Parsing;

namespace Crisp.Runtime
{
    [Serializable]
    class RuntimeErrorException : CrispException
    {
        public Position? Position { get; }

        public RuntimeErrorException(Position? position, string message)
            : base(message)
        {
            Position = position;
        }

        public RuntimeErrorException(string message)
            : this(null, message)
        {
        }

        public override string FormattedMessage()
        {
            if (Position == null)
            {
                return $"Runtime Error: {Message};";
            }
            else
            {
                return $"Runtime Error: {Message} @{Position}";
            }
        }
    }
}
