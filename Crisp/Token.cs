using System;

namespace Crisp
{
    class Token
    {
        public virtual int Lbp { get; } = 0;

        public virtual IExpression Nud(Parser parser)
        {
            throw new NotImplementedException();
        }

        public virtual IExpression Led(Parser parser, IExpression left)
        {
            throw new NotImplementedException();
        }
    }
}
