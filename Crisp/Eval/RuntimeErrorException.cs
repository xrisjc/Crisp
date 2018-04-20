using Crisp.Parsing;

namespace Crisp.Eval
{
    class RuntimeErrorException : CrispException
    {
        public Token Token { get; }

        public RuntimeErrorException(Token token, string message)
            : base(message)
        {
            Token = token;
        }

        public RuntimeErrorException(string message)
            : this(null, message)
        {
        }

        public override string FormattedMessage() =>
            $"Runtime Error: {Message} at {Token?.Position}";
    }
}
