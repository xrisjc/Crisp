using System.Collections.Generic;

namespace Crisp
{
    class Token
    {
        public virtual Precidence Lbp { get; } = Precidence.Lowest;

        public virtual IExpression Nud(Parser parser)
        {
            throw new SyntaxErrorException($"unexpected token '{this}'");
        }

        public virtual IExpression Led(Parser parser, IExpression left)
        {
            throw new SyntaxErrorException($"unexpected token '{this}'");
        }
    }

    abstract class TokenInfixOperator : Token
    {
        public abstract IExpression CreateExpression(IExpression left,
            IExpression right);

        public override IExpression Led(Parser parser, IExpression left)
        {
            var right = parser.ParseExpression(Lbp);
            return CreateExpression(left, right);
        }
    }

    class TokenAdd : TokenInfixOperator
    {
        public override Precidence Lbp => Precidence.Additive;

        public override IExpression CreateExpression(IExpression left,
            IExpression right)
        {
            return new ExpressionAdd(left, right);
        }
    }

    class TokenAssignment : Token
    {
        public override Precidence Lbp => Precidence.Assignment;

        public override IExpression Led(Parser parser, IExpression left)
        {
            if (left is ExpressionIdentifier identifier)
            {
                var right = parser.ParseExpression(Lbp);
                return new ExpressionAssignment(identifier.Name, right);
            }

            throw new SyntaxErrorException(
                "left hand side of assignment must be assignable");
        }
    }

    class TokenComma : Token { }

    class TokenElse : Token { }

    class TokenEndOfInput : Token { }

    class TokenEquals : TokenInfixOperator
    {
        public override Precidence Lbp => Precidence.Equality;

        public override IExpression CreateExpression(IExpression left,
            IExpression right)
        {
            return new ExpressionEquals(left, right);
        }
    }

    class TokenFn : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var parameters = new List<string>();
            string name = parser.Match<TokenIdentifier>(out var nameToken)
                ? nameToken.Name
                : null;
            parser.Expect<TokenLParen>();
            if (parser.Match<TokenIdentifier>(out var identifier))
            {
                parameters.Add(identifier.Name);
                while (parser.Match<TokenComma>())
                {
                    identifier = parser.Expect<TokenIdentifier>();
                    parameters.Add(identifier.Name);
                }
            }
            parser.Expect<TokenRParen>();
            var body = parser.ParseExpression();
            return new ExpressionFunction(name, parameters, body);
        }
    }

    class TokenIdentifier : Token
    {
        public string Name { get; }

        public TokenIdentifier(string name)
        {
            Name = name;
        }

        public override IExpression Nud(Parser parser)
        {
            return new ExpressionIdentifier(Name);
        }
    }

    class TokenIf : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var condition = parser.ParseExpression();
            var consequence = parser.ParseExpression();
            var alternative = parser.Match<TokenElse>()
                ? parser.ParseExpression()
                : ExpressionLiteralNull.Instance;

            return new ExpressionBranch(condition, consequence, alternative);
        }
    }

    class TokenLBrace : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var body = new List<IExpression>();
            while (!parser.Match<TokenRBrace>())
            {
                var expression = parser.ParseExpression();
                body.Add(expression);
            }
            return new ExpressionBlock(body);
        }
    }

    class TokenLet : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var identifier = parser.Expect<TokenIdentifier>();
            parser.Expect<TokenAssignment>();
            var value = parser.ParseExpression(Precidence.Assignment);
            return new ExpressionLet(identifier.Name, value);
        }
    }

    class TokenLiteral<T> : Token
    {
        T value;

        public TokenLiteral(T value)
        {
            this.value = value;
        }

        public override IExpression Nud(Parser parser)
        {
            return new ExpressionLiteral<T>(value);
        }
    }

    static class TokenLiteral
    {
        public static TokenLiteral<T> Create<T>(T value)
        {
            return new TokenLiteral<T>(value);
        }
    }

    class TokenLParen : Token
    {
        public override Precidence Lbp => Precidence.Parentheses;

        public override IExpression Nud(Parser parser)
        {
            var expression = parser.ParseExpression();
            parser.Expect<TokenRParen>();
            return expression;
        }

        public override IExpression Led(Parser parser, IExpression left)
        {
            var argumentExpressions = new List<IExpression>();

            if (!parser.Match<TokenRParen>())
            {
                do
                {
                    argumentExpressions.Add(parser.ParseExpression());
                }
                while (parser.Match<TokenComma>());

                parser.Expect<TokenRParen>();
            }

            return new ExpressionCall(left, argumentExpressions);
        }
    }

    class TokenMultiply : TokenInfixOperator
    {
        public override Precidence Lbp => Precidence.Multiplicitive;

        public override IExpression CreateExpression(IExpression left,
            IExpression right)
        {
            return new ExpressionMultiply(left, right);
        }
    }

    class TokenRBrace : Token { }

    class TokenRParen : Token { }

    class TokenWhile : Token
    {
        public override IExpression Nud(Parser parser)
        {
            var guard = parser.ParseExpression();
            var body = parser.ParseExpression();
            return new ExpressionWhile(guard, body);
        }
    }
}
