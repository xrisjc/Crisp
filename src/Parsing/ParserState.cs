namespace Crisp.Parsing
{
    class ParserState
    {
        Scanner scanner;

        public SymbolTable SymbolTable { get; private set; }

        public Token Current { get; private set; }

        public Token Peek { get; private set; }

        public ParserState()
        {
            SymbolTable = new SymbolTable();
        }

        public void NewCode(string code)
        {
            scanner = new Scanner(code);
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

        public void BeginScope()
        {
            SymbolTable = new SymbolTable(SymbolTable);
        }

        public void EndScope()
        {
            SymbolTable = SymbolTable.Outer;
        }

        public void CreateSymbol(string name, Position position, SymbolTag tag)
        {
            if (!SymbolTable.Create(name, tag))
            {
                throw new SyntaxErrorException($"symbol <{name}> has already been declared", position);
            }
        }

        public SymbolTag? SymbolLookup(string name)
        {
            return SymbolTable.Lookup(name);
        }
    }
}
