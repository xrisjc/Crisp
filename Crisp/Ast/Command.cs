namespace Crisp.Ast
{
    class Command : IExpression
    {
        public CommandType Type { get; }

        public List ArgumentExpressions { get; }

        public Command(CommandType type, List argumentExpressions)
        {
            Type = type;
            ArgumentExpressions = argumentExpressions;
        }
    }
}
