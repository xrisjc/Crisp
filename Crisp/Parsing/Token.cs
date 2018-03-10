namespace Crisp.Parsing
{
    class Token
    {
        public string Lexeme { get; set; }

        public TokenTag Tag { get; set; }

        public Position Position { get; set; }

        public override string ToString()
        {
            return $"Lexeme = {Lexeme}, Tag = {Tag}, Position = ({Position})";
        }
    }
}
