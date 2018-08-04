namespace Crisp.Parsing
{
    class ParserState
    {
        Scanner scanner;
        SymbolTable symbolTable;

        public Token Current { get; private set; }

        public Token Peek { get; private set; }

        public ParserState(Scanner scanner, SymbolTable symbolTable)
        {
            this.scanner = scanner;
            NextToken();
            NextToken();

            this.symbolTable = symbolTable;
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
            symbolTable = new SymbolTable(symbolTable);
        }

        public void EndScope()
        {
            symbolTable = symbolTable.Outer;
        }

        public void CreateSymbol(string symbol, Position position, SymbolTag tag)
        {
            if (!symbolTable.Create(symbol, tag))
            {
                throw new SyntaxErrorException($"symbol <{symbol}> has already been declared", position);
            }
        }

        public void CreateSymbol(Token token, SymbolTag tag)
        {
            CreateSymbol(token.Lexeme, token.Position, tag);
        }

        public SymbolTag? SymbolLookup(string symbol)
        {
            return symbolTable.Lookup(symbol);
        }
    }
}
