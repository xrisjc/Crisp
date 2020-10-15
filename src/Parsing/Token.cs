namespace Crisp.Parsing
{
    record Token(string Lexeme, TokenTag Tag, Position Position)
    {
        public static implicit operator bool(Token? token) => token != null;
    }
}
