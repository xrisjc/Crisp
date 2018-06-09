using System;

namespace Crisp.Parsing
{
    [Serializable]
    class SyntaxErrorException : CrispException
    {
        public Position Position { get; }

        public SyntaxErrorException(string message, Position position)
            : base(message)
        {
            Position = position;
        }

        public override string FormattedMessage() =>
            $"Syntax Error: {Message} @{Position.Line}:{Position.Column}";
    }
}
