﻿using Crisp.Ast;
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
                [TokenTag.Is] = OperatorBinaryTag.Is,
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
            var expressions = new List<IExpression>();

            while (!Match(TokenTag.EndOfInput))
            {
                var expr = Expression();
                expressions.Add(expr);
            }

            return new Program(expressions);
        }

        IExpression Expression()
        {
            if (Match(TokenTag.Var))
            {
                return Var();
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

            if (Match(TokenTag.For))
            {
                return For();
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

        Function Function()
        {
            var parameters = Parameters();
            var body = ExpectBlock();
            return new Function(parameters, body);
        }

        IExpression If()
        {
            (IExpression, Block) Branch()
            {
                var condition = Expression();
                var consequence = ExpectBlock();
                return (condition, consequence);
            }

            var branches = new List<(IExpression, Block)>();

            branches.Add(Branch());

            while (Current.Tag == TokenTag.Else && Peek.Tag == TokenTag.If)
            {
                NextToken();
                NextToken();
                branches.Add(Branch());
            }

            var elseBlock = Match(TokenTag.Else) ? ExpectBlock() : null;

            return new Conditional(branches, elseBlock);
        }

        IExpression While()
        {
            var guard = Expression();
            var body = ExpectBlock();
            return new While(guard, body);
        }

        IExpression For()
        {
            var variableToken = Expect(TokenTag.Identifier);
            var variable = new Identifier(
                variableToken.Position,
                variableToken.Lexeme);

            // Use the position of the `in` token as the position of the `for`
            // loop because all the runtime errors involve the iterable object
            // not supporting the iterable interface.
            var inToken = Expect(TokenTag.In);
            var position = inToken.Position;

            var iterable = Expression();

            var body = ExpectBlock();

            return new For(position, variable, iterable, body);
        }

        Block ExpectBlock()
        {
            Expect(TokenTag.LBrace);
            return Block();
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

            if (Match(TokenTag.Assignment) is Token assignmentToken)
            {
                if (left is Identifier identifier)
                {
                    var right = Expression();
                    return new AssignmentIdentifier(identifier, right);
                }

                if (left is Index index)
                {
                    var right = Expression();
                    return new AssignmentIndex(index, right);
                }

                if (left is Refinement refinement)
                {
                    var right = Expression();
                    return new AssignmentRefinement(refinement, right);
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
            while (Match(TokenTag.GreaterThan, TokenTag.GreaterThanOrEqualTo, TokenTag.LessThan, TokenTag.LessThanOrEqualTo, TokenTag.Is) is Token token)
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
            while (true)
            {
                if (Match(TokenTag.LParen) is Token tokenCall)
                    left = new Call(tokenCall.Position, left, Arguments(TokenTag.RParen));
                else if (Match(TokenTag.LBracket) is Token tokenIndex)
                {
                    var at = Expression();
                    Expect(TokenTag.RBracket);
                    left = new Index(tokenIndex.Position, left, at);
                }
                else if (Match(TokenTag.Dot))
                {
                    var nameToken = Expect(TokenTag.Identifier);
                    var name = new Identifier(
                        nameToken.Position,
                        nameToken.Lexeme);
                    left = new Refinement(left, name);
                }
                else
                    break;
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

            if (Match(TokenTag.Self) is Token tokenSelf)
            {
                return new Self(tokenSelf.Position);
            }

            if (Match(TokenTag.LBracket))
            {
                var items = Arguments(endToken: TokenTag.RBracket);
                return new LiteralList(items);
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

        List<IExpression> Arguments(TokenTag endToken)
        {
            var arguments = new List<IExpression>();
            if (!Match(endToken))
            {
                do
                {
                    var argument = Expression();
                    arguments.Add(argument);
                }
                while (Match(TokenTag.Comma));
                Expect(endToken);
            }
            return arguments;
        }

        IExpression Write()
        {
            Expect(TokenTag.LParen);
            var arguments = Arguments(TokenTag.RParen);
            return new Write(arguments);
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
