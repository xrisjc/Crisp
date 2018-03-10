namespace Crisp.Parsing
{
    class Token
    {
        public string Lexeme { get; set; }

        public TokenTag Tag { get; set; }

        public Position Position { get; set; }
    }
}
