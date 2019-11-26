namespace Crisp.Parsing
{
    class ParserState
    {
        Scanner scanner;

        public Token Current { get; private set; }

        public Token Peek { get; private set; }

        public ParserState(Scanner scanner)
        {
            this.scanner = scanner;
            Current = scanner.NextToken();
            Peek = scanner.NextToken();
        }

        private void NextToken()
        {
            Current = Peek;
            Peek = scanner.NextToken();
        }

        public Token? Match2(TokenTag tag)
        {
            if (Current.Tag == tag)
            {
                var token = Current;
                NextToken();
                return token;
            }
            else
                return null;
        }

        public Token? Match2(params TokenTag[] tags)
        {
            foreach (var tag in tags)
                if (Match2(tag) is Token token)
                    return token;

            return null;
        }

        public bool Match(TokenTag tag)
        {
            return Match2(tag) != null;
        }

        public Token Expect(TokenTag tag)
        {
            if (Match2(tag) is Token token)
                return token;
            else
                throw new SyntaxErrorException(
                    $"exepected, but didn't match, token {tag}",
                    Current.Position);
        }
    }
}
