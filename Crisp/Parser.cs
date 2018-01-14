using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IExpression ParseExpression(int rbp = 0)
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

        void NextToken()
        {
            current = peek;
            peek = lexer.NextToken();
        }
    }
}
