namespace Crisp.Parsing
{
    class ParserState
    {
        readonly Scanner scanner;

        public Token Current { get; private set; }

        public Token Peek { get; private set; }

        public ParserState(Scanner scanner)
        {
            this.scanner = scanner;
            NextToken();
            NextToken();
        }

        public void NextToken()
        {
            Current = Peek;
            Peek = scanner.NextToken();
        }

        public bool Match(out Token token, TokenTag tag)
        {
            if (Current.Tag == tag)
            {
                token = Current;
                NextToken();
                return true;
            }
            else
            {
                token = null;
                return false;
            }
        }

        public bool Match(TokenTag tag)
        {
            return Match(out var token, tag);
        }

        public bool Match(out Token token, params TokenTag[] tags)
        {
            foreach (var tag in tags)
            {
                if (Match(out token, tag))
                {
                    return true;
                }
            }
            token = null;
            return false;
        }

        public Token Expect(TokenTag tag)
        {
            if (Current.Tag == tag)
            {
                var token = Current;
                NextToken();
                return token;
            }
            else
            {
                throw new SyntaxErrorException($"exepected, but didn't match, token {tag}", Current.Position);
            }
        }
    }
}
