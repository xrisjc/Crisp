namespace Crisp
{
    class Parser
    {
        readonly Lexer lexer;
        Token current = null;
        Token peek = null;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            NextToken();
            NextToken();
        }

        public IExpression Parse(Precidence rbp = Precidence.Lowest)
        {
            var t = current;
            NextToken();
            var left = t.Nud(this);
            while (rbp < current.Lbp)
            {
                t = current;
                NextToken();
                left = t.Led(this, left);
            }
            return left;
        }

        public bool Match<T>()
            where T : Token
        {
            if (current is T)
            {
                NextToken();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Match<T>(out T token)
            where T : Token
        {
            if (current is T)
            {
                token = current as T;
                NextToken();
                return true;
            }
            else
            {
                token = null;
                return false;
            }
        }

        public T Expect<T>()
            where T : Token
        {
            if (current is T)
            {
                var token = current as T;
                NextToken();
                return token;
            }
            else
            {
                throw new SyntaxErrorException($"Exepected token of type {typeof(T)}");
            }
        }

        void NextToken()
        {
            current = peek;
            peek = lexer.NextToken();
        }
    }
}
