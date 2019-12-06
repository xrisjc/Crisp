using Crisp.Ast;
using System.Collections.Generic;
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
            return parser.Program();
        }

        public Parser(Scanner scanner)
            : base(scanner)
        {
        }

        Program Program()
        {
            var program = new Program();

            while (!Match(TokenTag.EndOfInput))
            {
                if (Match(TokenTag.Function))
                {
                    var fn = Function();
                    program.Add(fn);
                }
                else
                {
                    var expr = Expression();
                    program.Add(expr);
                }
            }

            return program;
        }

        Function Function()
        {
            var name = Expect(TokenTag.Identifier);
            var parameters = Parameters();
            Expect(TokenTag.LBrace);
            var body = Block();

            var fn = new Function(
                new Identifier(name.Position, name.Lexeme),
                parameters,
                body);

            return fn;
        }

        IExpression Expression()
        {
            if (Match(TokenTag.Var))
            {
                return Var();
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

        IExpression Var()
        {
            var name = Expect(TokenTag.Identifier);

            Expect(TokenTag.Assignment);
            var initialValue = Expression();

            return new Var(
                new Identifier(name.Position, name.Lexeme),
                initialValue);
        }

        IExpression If()
        {
            IEnumerable<Branch> Branches()
            {
                IExpression Consequence()
                {
                    Expect(TokenTag.LBrace);
                    return Block();
                }

                yield return new Branch(Expression(), Consequence());

                bool sawIfElse;
                do
                {
                    if (Match(TokenTag.Else))
                    {
                        sawIfElse = Match(TokenTag.If);
                        yield return new Branch(sawIfElse ? Expression() : Literal.True,
                                                Consequence());
                    }
                    else
                    {
                        sawIfElse = false;
                    }
                }
                while (sawIfElse);
            }

            return new Condition(Branches().ToList());
        }

        IExpression While()
        {
            var guard = Expression();
            Expect(TokenTag.LBrace);
            var body = Block();
            return new While(guard, body);
        }

        Block Block()
        {
            var body = new List<IExpression>();
            while (!Match(TokenTag.RBrace))
            {
                var expr = Expression();
                body.Add(expr);
            }
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

            if (Match2(TokenTag.Assignment) is Token assignmentToken)
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
            while (Match2(TokenTag.Or) is Token token)
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
            while (Match2(TokenTag.And) is Token token)
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
            while (Match2(TokenTag.Equals, TokenTag.InequalTo) is Token token)
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
            while (Match2(TokenTag.GreaterThan, TokenTag.GreaterThanOrEqualTo, TokenTag.LessThan, TokenTag.LessThanOrEqualTo) is Token token)
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
            while (Match2(TokenTag.Add, TokenTag.Subtract) is Token token)
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
            while (Match2(TokenTag.Multiply, TokenTag.Divide, TokenTag.Mod) is Token token)
            {
                var op = tokenOp[token.Tag];
                var right = Unary();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Unary()
        {
            if (Match2(TokenTag.Subtract) is Token tokenSubtract)
            {
                var expression = Expression();
                return new OperatorUnary(tokenSubtract.Position, OperatorUnaryTag.Neg, expression);
            }

            if (Match2(TokenTag.Not) is Token tokenNot)
            {
                var expression = Expression();
                return new OperatorUnary(tokenNot.Position, OperatorUnaryTag.Not, expression);
            }

            return Primary();
        }

        IExpression Primary()
        {
            if (Current.Tag == TokenTag.Identifier &&
                Peek.Tag == TokenTag.LParen)
            {
                var name = Current;
                NextToken();
                NextToken();
                var arguments = Arguments();
                return new Call(name.Position, name.Lexeme, arguments);
            }

            if (Match2(TokenTag.Float) is Token token)
            {
                if (double.TryParse(token.Lexeme, out var value))
                {
                    return new Literal(value);
                }
                throw new SyntaxErrorException(
                    $"Unable to convert <{token.Lexeme}> into a 64 bit floating bit",
                    token.Position);
            }

            if (Match2(TokenTag.Integer) is Token tokenInteger)
            {
                if (int.TryParse(tokenInteger.Lexeme, out var value))
                {
                    return new Literal(value);
                }
                throw new SyntaxErrorException(
                    $"Unable to convert <{tokenInteger.Lexeme}> into a 32 bit integer.",
                    tokenInteger.Position);
            }

            if (Match2(TokenTag.String) is Token tokenString)
            {
                var str = ParseString(tokenString.Lexeme, tokenString.Position);
                return new Literal(str);
            }

            if (Match(TokenTag.True))
            {
                return Literal.True;
            }

            if (Match(TokenTag.False))
            {
                return Literal.False;
            }

            if (Match2(TokenTag.Identifier) is Token tokenIdentifier)
            {
                return new Identifier(tokenIdentifier.Position, tokenIdentifier.Lexeme);
            }

            if (Match(TokenTag.Null))
            {
                return LiteralNull.Instance;
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

        List<IExpression> Arguments()
        {
            var arguments = new List<IExpression>();
            if (!Match(TokenTag.RParen))
            {
                do
                {
                    var argument = Expression();
                    arguments.Add(argument);
                }
                while (Match(TokenTag.Comma));
                Expect(TokenTag.RParen);
            }
            return arguments;
        }

        IExpression Write()
        {
            Expect(TokenTag.LParen);
            var arguments = Arguments();
            return new Write(arguments);
        }

        static string ParseString(string lexeme, Position position)
        {
            // Assume the scanner provides us with a string that starts and
            // ends with single quote.

            var sb = new StringBuilder();
            var state = 's';

            for (var i = 1; i < lexeme.Length - 1; i++)
            {
                switch (lexeme[i])
                {
                    case '\\' when state == 's':
                        state = 'e';
                        break;

                    case '\\' when state == 'e':
                        sb.Append('\\');
                        state = 's';
                        break;

                    case 'n' when state == 'e':
                        sb.Append('\n');
                        state = 's';
                        break;

                    case char c when state == 's':
                        sb.Append(c);
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
