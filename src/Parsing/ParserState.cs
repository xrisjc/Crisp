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

        public void NextToken()
        {
            Current = Peek;
            Peek = scanner.NextToken();
        }

        public bool CurrentIs(TokenTag tag) => Current.Tag == tag;

        public bool CurrentIs(params TokenTag[] tags)
        {
            foreach (var tag in tags)
                if (CurrentIs(tag))
                    return true;
            return false;
        }

        public Token? Match(TokenTag tag)
        {
            if (CurrentIs(tag))
            {
                var token = Current;
                NextToken();
                return token;
            }
            else
                return null;
        }

        public Token? Match(params TokenTag[] tags)
        {
            foreach (var tag in tags)
                if (Match(tag) is Token token)
                    return token;

            return null;
        }

        public Token Expect(TokenTag tag)
        {
            if (Match(tag) is Token token)
                return token;
            else
                throw new SyntaxErrorException(
                    $"expected, but didn't match, token {tag}",
                    Current.Position);
        }
    }
}
