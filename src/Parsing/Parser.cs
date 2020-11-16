using Crisp.Ast;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Crisp.Parsing
{
    class Parser : ParserState
    {
        static Dictionary<TokenTag, OperatorBinaryTag> tokenOp =
            new Dictionary<TokenTag, OperatorBinaryTag>
            {
                [TokenTag.Add] = OperatorBinaryTag.Add,
                [TokenTag.And] = OperatorBinaryTag.And,
                [TokenTag.Equals] = OperatorBinaryTag.Eq,
                [TokenTag.Divide] = OperatorBinaryTag.Div,
                [TokenTag.GreaterThan] = OperatorBinaryTag.Gt,
                [TokenTag.GreaterThanOrEqualTo] = OperatorBinaryTag.GtEq,
                [TokenTag.InequalTo] = OperatorBinaryTag.Neq,
                [TokenTag.LessThan] = OperatorBinaryTag.Lt,
                [TokenTag.LessThanOrEqualTo] = OperatorBinaryTag.LtEq,
                [TokenTag.Mod] = OperatorBinaryTag.Mod,
                [TokenTag.Multiply] = OperatorBinaryTag.Mul,
                [TokenTag.Or] = OperatorBinaryTag.Or,
                [TokenTag.Subtract] = OperatorBinaryTag.Sub,
            };

        public static IExpression Parse(string code)
        {
            var scanner = new Scanner(code);
            var parser = new Parser(scanner);
            var program = parser.Program();
            return program;
        }

        public Parser(Scanner scanner)
            : base(scanner)
        {
        }

        IExpression Program()
        {
            var symbols = ImmutableList<string>.Empty;
            var body = ExpressionList(symbols, TokenTag.EndOfInput);
            Expect(TokenTag.EndOfInput);
            return body;
        }

        IExpression ExpressionList(
            ImmutableList<string> symbols,
            params TokenTag[] stopTokens)
        {
            var body = new List<IExpression>();
            while (!CurrentIs(stopTokens))
            {
                var expr = Expression(symbols);
                body.Add(expr);
            }

            if (body.Count == 0)
                return new LiteralNull();
            else
            {
                var e = body[^1];
                for (var i = 2; i <= body.Count; i++)
                    e = new ExpressionPair(body[^i], e);
                return e;
            }
        }

        IExpression Expression(ImmutableList<string> symbols)
        {
            if (Match(TokenTag.Let))
            {
                if (Match(TokenTag.Rec))
                    return LetRec(symbols);
                else
                    return Let(symbols);
            }

            if (Match(TokenTag.Fn))
            {
                return Function(symbols);
            }

            if (Match(TokenTag.If))
            {
                return If(symbols);
            }

            if (Match(TokenTag.While))
            {
                return While(symbols);
            }

            if (Match(TokenTag.LBrace))
            {
                return Block(symbols);
            }

            return Assignment(symbols);
        }

        IExpression Let(ImmutableList<string> symbols)
        {
            IExpression InitialValue(ImmutableList<string> symbols)
            {
                if (CurrentIs(TokenTag.LParen))
                    return Function(symbols);
                Expect(TokenTag.Assignment);
                return Expression(symbols);
            }
            
            var id = Expect(TokenTag.Identifier);
            var initialValue = InitialValue(symbols);
            symbols = symbols.Add(id.Lexeme);
            var body = ExpressionList(symbols, TokenTag.RParen, TokenTag.RBrace, TokenTag.EndOfInput);
            return new Let(initialValue, body);
        }

        IExpression LetRec(ImmutableList<string> symbols)
        {
            var id = Expect(TokenTag.Identifier);
            symbols = symbols.Add(id.Lexeme);
            var callable = Function(symbols);
            var body = ExpressionList(symbols, TokenTag.RParen, TokenTag.RBrace, TokenTag.EndOfInput);
            return new LetRec(callable, body);
        }

        IExpression Function(ImmutableList<string> symbols)
        {
            IEnumerable<string> Parameters()
            {
                Expect(TokenTag.LParen);
                if (Match(TokenTag.RParen))
                {
                    // Add a "discard" parameter for no parameter function.
                    yield return "_";
                }
                else
                {
                    do
                    {
                        var id = Expect(TokenTag.Identifier);
                        yield return id.Lexeme;
                    }
                    while (Match(TokenTag.Comma));
                    Expect(TokenTag.RParen);
                }
            }

            var parameters = Parameters().ToList();
            foreach (var p in parameters)
                symbols = symbols.Add(p);
            
            var body = ExpectBlock(symbols);

            var fn = new Function(body);
            for (var i = 1; i < parameters.Count; i++)
                fn = new Function(fn);
            
            return fn;
        }

        IExpression If(ImmutableList<string> symbols)
        {
            var condition = Expression(symbols);
            var consequence = ExpectBlock(symbols);

            IExpression alternative;
            if (Current.Tag == TokenTag.Else && Peek.Tag == TokenTag.If)
            {
                NextToken();
                NextToken();
                alternative = If(symbols);
            }
            else if (Match(TokenTag.Else))
            {
                alternative = ExpectBlock(symbols);
            }
            else
            {
                alternative = new LiteralNull();
            }
            
            return new Conditional(condition, consequence, alternative);
        }

        IExpression While(ImmutableList<string> symbols)
        {
            var guard = Expression(symbols);
            var body = ExpectBlock(symbols);
            return new While(guard, body);
        }

        IExpression ExpectBlock(ImmutableList<string> symbols)
        {
            Expect(TokenTag.LBrace);
            return Block(symbols);
        }

        IExpression Block(ImmutableList<string> symbols)
        {
            var body = ExpressionList(symbols, TokenTag.RBrace);
            Expect(TokenTag.RBrace);
            return body;
        }

        IExpression Assignment(ImmutableList<string> symbols)
        {
            var left = LogicalOr(symbols);

            if (Match(TokenTag.Assignment) is Token assignmentToken)
            {
                if (left is Identifier identifier)
                {
                    var right = Expression(symbols);
                    return new AssignmentIdentifier(identifier, right);
                }

                throw new SyntaxErrorException(
                    "Left hand side of assignment must be assignable,",
                    assignmentToken.Position);
            }

            return left;
        }

        IExpression LogicalOr(ImmutableList<string> symbols)
        {
            var left = LogicalAnd(symbols);
            while (Match(TokenTag.Or) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = LogicalAnd(symbols);
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression LogicalAnd(ImmutableList<string> symbols)
        {
            var left = Equality(symbols);
            while (Match(TokenTag.And) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Equality(symbols);
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Equality(ImmutableList<string> symbols)
        {
            var left = Relation(symbols);
            while (Match(TokenTag.Equals, TokenTag.InequalTo) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Relation(symbols);
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Relation(ImmutableList<string> symbols)
        {
            var left = Addition(symbols);
            while (Match(TokenTag.GreaterThan, TokenTag.GreaterThanOrEqualTo, TokenTag.LessThan, TokenTag.LessThanOrEqualTo) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Addition(symbols);
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Addition(ImmutableList<string> symbols)
        {
            var left = Multiplication(symbols);
            while (Match(TokenTag.Add, TokenTag.Subtract) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Multiplication(symbols);
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Multiplication(ImmutableList<string> symbols)
        {
            var left = Unary(symbols);
            while (Match(TokenTag.Multiply, TokenTag.Divide, TokenTag.Mod) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Unary(symbols);
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Unary(ImmutableList<string> symbols)
        {
            if (Match(TokenTag.Subtract) is Token tokenSubtract)
            {
                var expression = Unary(symbols);
                return new OperatorUnary(tokenSubtract.Position, OperatorUnaryTag.Neg, expression);
            }

            if (Match(TokenTag.Bang) is Token tokenNot)
            {
                var expression = Unary(symbols);
                return new OperatorUnary(tokenNot.Position, OperatorUnaryTag.Not, expression);
            }

            return Call(symbols);
        }

        IExpression Call(ImmutableList<string> symbols)
        {
            var left = Primary(symbols);
            while (Match(TokenTag.LParen) is Token tokenCall)
            {
                if (Match(TokenTag.RParen))
                    left = new FunctionCall(tokenCall.Position, left, new LiteralNull());
                else
                {
                    do
                    {
                        var argument = Expression(symbols);
                        left = new FunctionCall(tokenCall.Position, left, argument);
                    }
                    while (Match(TokenTag.Comma));
                    Expect(TokenTag.RParen);
                }
            }
            return left;
        }

        IExpression Primary(ImmutableList<string> symbols)
        {
            if (Match(TokenTag.Number) is Token token)
            {
                if (double.TryParse(token.Lexeme, out var value))
                {
                    return new LiteralNumber(value);
                }
                throw new SyntaxErrorException(
                    $"Unable to convert <{token.Lexeme}> into a 64 bit floating bit",
                    token.Position);
            }

            if (Match(TokenTag.String) is Token tokenString)
            {
                var str = ParseString(tokenString.Lexeme, tokenString.Position);
                return new LiteralString(str);
            }

            if (Match(TokenTag.True))
            {
                return new LiteralBool(true);
            }

            if (Match(TokenTag.False))
            {
                return new LiteralBool(false);
            }

            if (Match(TokenTag.Identifier) is Token tokenIdentifier)
            {
                var name = tokenIdentifier.Lexeme;
                var depth = symbols.FindLastIndex(sym => sym == name);
                if (depth < 0)
                    throw new SyntaxErrorException(
                        $"undefined symbol `{name}`",
                        tokenIdentifier.Position);

                return new Identifier(depth);
            }

            if (Match(TokenTag.Null))
            {
                return new LiteralNull();
            }

            if (Match(TokenTag.LParen))
            {
                var expression = Expression(symbols);
                Expect(TokenTag.RParen);
                return expression;
            }

            if (Match(TokenTag.Write))
            {
                return Write(symbols);
            }

            throw new SyntaxErrorException($"unexpected token '{Current.Tag}'", Current.Position);
        }

        IExpression Write(ImmutableList<string> symbols)
        {
            Expect(TokenTag.LParen);
            if (Match(TokenTag.RParen))
                throw new SyntaxErrorException(
                    $"write requires at least one argument",
                    Current.Position);
            var stack = new Stack<IExpression>();
            do
            {
                var value = Expression(symbols);
                stack.Push(value);
            }
            while (Match(TokenTag.Comma));
            Expect(TokenTag.RParen);

            IExpression write = new Write(stack.Pop());
            while (stack.Count > 0)
                write = new ExpressionPair(new Write(stack.Pop()), write);

            return write;
        }

        static string ParseString(string lexeme, Position position)
        {
            // Assume the scanner provides us with a string that starts and
            // ends with double quotes.

            bool IsEscapeCode(char c) => c == 'n' || c == '\\';
            char Escape(char code) => code switch { 'n' => '\n', char c => c };


            var sb = new StringBuilder();
            var state = 's';

            for (var i = 1; i < lexeme.Length - 1; i++)
            {
                switch (lexeme[i])
                {
                    case '\\' when state == 's':
                        state = 'e';
                        break;

                    case char c when state == 's':
                        sb.Append(c);
                        break;

                    case char c when IsEscapeCode(c) && state == 'e':
                        sb.Append(Escape(c));
                        state = 's';
                        break;

                    case char c when state == 'e':
                        throw new SyntaxErrorException($"unexpected escape code '\\{c}' in string", position);

                    default:
                        throw new SyntaxErrorException($"error parsing string", position);
                }
            }

            if (state == 'e')
            {
                throw new SyntaxErrorException($"string uses '\\' with no escape", position);
            }

            return sb.ToString();
        }
    }
}
