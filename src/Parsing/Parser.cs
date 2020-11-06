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

        public static Program Parse(string code)
        {
            var scanner = new Scanner(code);
            var parser = new Parser(scanner);
            var program = parser.Program();
            Resolver.Resolve(program, ImmutableList<string>.Empty);
            return program;
        }

        public Parser(Scanner scanner)
            : base(scanner)
        {
        }

        Program Program()
        {
            var body = ExpressionList(TokenTag.EndOfInput);
            Expect(TokenTag.EndOfInput);
            return new Program(body);
        }

        IExpression ExpressionList(params TokenTag[] stopTokens)
        {
            var body = new List<IExpression>();
            while (!CurrentIs(stopTokens))
            {
                var expr = Expression();
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

        IExpression Expression()
        {
            if (Match(TokenTag.Let))
            {
                if (Match(TokenTag.Rec))
                    return LetRec();
                else
                    return Let();
            }

            if (Match(TokenTag.Fn))
            {
                return Function();
            }

            if (Match(TokenTag.If))
            {
                return If();
            }

            if (Match(TokenTag.While))
            {
                return While();
            }

            if (Match(TokenTag.LBrace))
            {
                return Block();
            }

            return Assignment();
        }

        IExpression Let()
        {
            IExpression InitialValue()
            {
                if (CurrentIs(TokenTag.LParen))
                    return Function();
                Expect(TokenTag.Assignment);
                return Expression();
            }
            
            var name = Expect(TokenTag.Identifier);
            var initialValue = InitialValue();
            var body = ExpressionList(TokenTag.RParen, TokenTag.RBrace, TokenTag.EndOfInput);
            return new Let(
                new Identifier(name.Position, name.Lexeme),
                initialValue,
                body);
        }

        IExpression LetRec()
        {
            var name = Expect(TokenTag.Identifier).Lexeme;
            var callable = Function();
            var body = ExpressionList(TokenTag.RParen, TokenTag.RBrace, TokenTag.EndOfInput);
            return new LetRec(name, callable, body);
        }

        IExpression Function()
        {
            var parameters = Parameters();
            var body = ExpectBlock();

            if (parameters.Count == 0)
                return new Procedure(body);
            else
            {
                var fn = new Function(parameters[^1], body);
                for (var i = 2; i <= parameters.Count; i++)
                    fn = new Function(parameters[^i], fn);
                return fn;
            }
        }

        IExpression If()
        {
            var condition = Expression();
            var consequence = ExpectBlock();

            IExpression alternative;
            if (Current.Tag == TokenTag.Else && Peek.Tag == TokenTag.If)
            {
                NextToken();
                NextToken();
                alternative = If();
            }
            else if (Match(TokenTag.Else))
            {
                alternative = ExpectBlock();
            }
            else
            {
                alternative = new LiteralNull();
            }
            
            return new Conditional(condition, consequence, alternative);
        }

        IExpression While()
        {
            var guard = Expression();
            var body = ExpectBlock();
            return new While(guard, body);
        }

        IExpression ExpectBlock()
        {
            Expect(TokenTag.LBrace);
            return Block();
        }

        IExpression Block()
        {
            var body = ExpressionList(TokenTag.RBrace);
            Expect(TokenTag.RBrace);
            return new Block(body);
        }

        List<Identifier> Parameters()
        {
            Expect(TokenTag.LParen);
            if (Match(TokenTag.RParen))
            {
                return new List<Identifier>();
            }
            else
            {
                var parameters = IdentifierList().ToList();
                Expect(TokenTag.RParen);
                return parameters;
            }
        }

        IEnumerable<Identifier> IdentifierList()
        {
            do
            {
                var name = Expect(TokenTag.Identifier);
                yield return new Identifier(name.Position, name.Lexeme);
            }
            while (Match(TokenTag.Comma));
        }

        IExpression Assignment()
        {
            var left = LogicalOr();

            if (Match(TokenTag.Assignment) is Token assignmentToken)
            {
                if (left is Identifier identifier)
                {
                    var right = Expression();
                    return new AssignmentIdentifier(identifier, right);
                }

                throw new SyntaxErrorException(
                    "Left hand side of assignment must be assignable,",
                    assignmentToken.Position);
            }

            return left;
        }

        IExpression LogicalOr()
        {
            var left = LogicalAnd();
            while (Match(TokenTag.Or) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = LogicalAnd();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression LogicalAnd()
        {
            var left = Equality();
            while (Match(TokenTag.And) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Equality();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Equality()
        {
            var left = Relation();
            while (Match(TokenTag.Equals, TokenTag.InequalTo) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Relation();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Relation()
        {
            var left = Addition();
            while (Match(TokenTag.GreaterThan, TokenTag.GreaterThanOrEqualTo, TokenTag.LessThan, TokenTag.LessThanOrEqualTo) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Addition();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Addition()
        {
            var left = Multiplication();
            while (Match(TokenTag.Add, TokenTag.Subtract) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Multiplication();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Multiplication()
        {
            var left = Unary();
            while (Match(TokenTag.Multiply, TokenTag.Divide, TokenTag.Mod) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Unary();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Unary()
        {
            if (Match(TokenTag.Subtract) is Token tokenSubtract)
            {
                var expression = Unary();
                return new OperatorUnary(tokenSubtract.Position, OperatorUnaryTag.Neg, expression);
            }

            if (Match(TokenTag.Bang) is Token tokenNot)
            {
                var expression = Unary();
                return new OperatorUnary(tokenNot.Position, OperatorUnaryTag.Not, expression);
            }

            return Call();
        }

        IExpression Call()
        {
            var left = Primary();
            while (Match(TokenTag.LParen) is Token tokenCall)
            {
                if (Match(TokenTag.RParen))
                    left = new ProcedureCall(tokenCall.Position, left);
                else
                {
                    do
                    {
                        var argument = Expression();
                        left = new FunctionCall(tokenCall.Position, left, argument);
                    }
                    while (Match(TokenTag.Comma));
                    Expect(TokenTag.RParen);
                }
            }
            return left;
        }

        IExpression Primary()
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
                return new Identifier(tokenIdentifier.Position, tokenIdentifier.Lexeme);
            }

            if (Match(TokenTag.Null))
            {
                return new LiteralNull();
            }

            if (Match(TokenTag.LParen))
            {
                var expression = Expression();
                Expect(TokenTag.RParen);
                return expression;
            }

            if (Match(TokenTag.Write))
            {
                return Write();
            }

            throw new SyntaxErrorException($"unexpected token '{Current.Tag}'", Current.Position);
        }

        IExpression Write()
        {
            Expect(TokenTag.LParen);
            if (Match(TokenTag.RParen))
                throw new SyntaxErrorException(
                    $"write requires at least one argument",
                    Current.Position);
            var stack = new Stack<IExpression>();
            do
            {
                var value = Expression();
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
