using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Parsing
{
    class Parser
    {
        static Dictionary<TokenTag, OperatorInfix> tokenOp =
            new Dictionary<TokenTag, OperatorInfix>
            {
                [TokenTag.Add] = OperatorInfix.Add,
                [TokenTag.And] = OperatorInfix.And,
                [TokenTag.Equals] = OperatorInfix.Eq,
                [TokenTag.Divide] = OperatorInfix.Div,
                [TokenTag.GreaterThan] = OperatorInfix.Gt,
                [TokenTag.GreaterThanOrEqualTo] = OperatorInfix.GtEq,
                [TokenTag.InequalTo] = OperatorInfix.Neq,
                [TokenTag.LessThan] = OperatorInfix.Lt,
                [TokenTag.LessThanOrEqualTo] = OperatorInfix.LtEq,
                [TokenTag.Mod] = OperatorInfix.Mod,
                [TokenTag.Multiply] = OperatorInfix.Mul,
                [TokenTag.Or] = OperatorInfix.Or,
                [TokenTag.Subtract] = OperatorInfix.Sub,
            };

        static Dictionary<TokenTag, CommandType> tokenCommand =
            new Dictionary<TokenTag, CommandType>
            {
                [TokenTag.Push] = CommandType.Push,
                [TokenTag.ReadLn] = CommandType.ReadLn,
                [TokenTag.WriteLn] = CommandType.WriteLn,
            };

        readonly Scanner scanner;
        Token current = null;
        Token peek = null;

        public Parser(Scanner scanner)
        {
            this.scanner = scanner;
            NextToken();
            NextToken();
        }

        void NextToken()
        {
            current = peek;
            peek = scanner.NextToken();
        }

        bool Match(out Token token, TokenTag tag)
        {
            if (current.Tag == tag)
            {
                token = current;
                NextToken();
                return true;
            }
            else
            {
                token = null;
                return false;
            }
        }

        bool Match(TokenTag tag)
        {
            return Match(out var token, tag);
        }

        bool Match(out Token token, params TokenTag[] tags)
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

        Token Expect(TokenTag tag)
        {
            if (current.Tag == tag)
            {
                var token = current;
                NextToken();
                return token;
            }
            else
            {
                throw new SyntaxErrorException(
                    $"exepected, but didn't match, token {tag}", current.Position);
            }
        }

        public List<IExpression> Program()
        {
            var expressions = new List<IExpression>();
            while (current.Tag != TokenTag.EndOfInput)
            {
                var expression = Expression();
                expressions.Add(expression);
            }
            return expressions;
        }

        IExpression Expression()
        {
            if (Match(TokenTag.Let))
            {
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

            if (Match(TokenTag.For))
            {
                return For();
            }

            if (Match(TokenTag.Begin))
            {
                return Block();
            }

            return Assignment();
        }

        IExpression Let()
        {
            var nameToken = Expect(TokenTag.Identifier);
            var identifier = new Identifier(nameToken.Position, nameToken.Lexeme);
            Expect(TokenTag.Assignment);
            var value = Expression();
            return new Let(identifier, value);
        }

        IExpression Function()
        {
            // Function name
            Match(out var nameToken, TokenTag.Identifier);

            // Function parameter list
            List<string> parameters = Parameters();

            // Function body
            var body = Expression();

            // Create AST
            var function = new Function(parameters, body);
            if (nameToken != null)
            {
                var identifier = new Identifier(nameToken.Position, nameToken.Lexeme);
                return new Let(identifier, function);
            }
            else
            {
                return function;
            }
        }

        IExpression If()
        {
            var condition = Expression();
            Expect(TokenTag.Then);
            var consequence = Expression();
            var alternative = Match(TokenTag.Else)
                ? Expression()
                : LiteralNull.Instance;
            return new Branch(condition, consequence, alternative);
        }

        IExpression While()
        {
            var guard = Expression();
            Expect(TokenTag.Do);
            var body = Expression();
            return new While(guard, body);
        }

        IExpression For()
        {
            var varToken = Expect(TokenTag.Identifier);
            if (Match(TokenTag.Assignment))
            {
                // Regular for loop.
                var start = Expression();
                Expect(TokenTag.To);
                var end = Expression();
                Expect(TokenTag.Do);
                var body = Expression();
                return new For(varToken.Lexeme, start, end, body);
            }
            else
            {
                Expect(TokenTag.In);
                var sequence = Expression();
                Expect(TokenTag.Do);
                var body = Expression();
                return new ForIn(varToken.Lexeme, sequence, body);
            }
        }

        IExpression Block()
        {
            var body = new List<IExpression>();
            while (!Match(TokenTag.End))
            {
                var expr = Expression();
                body.Add(expr);
            }
            return new Block(body);
        }

        List<string> Parameters()
        {
            var parameters = new List<string>();
            Expect(TokenTag.LParen);
            if (!Match(TokenTag.RParen))
            {
                do
                {
                    var parameterToken = Expect(TokenTag.Identifier);
                    parameters.Add(parameterToken.Lexeme);
                }
                while (Match(TokenTag.Comma));
                Expect(TokenTag.RParen);
            }

            return parameters;
        }

        IExpression Assignment()
        {
            var left = LogicalOr();

            if (Match(out var token, TokenTag.Assignment))
            {
                var right = Expression();

                if (left is Identifier identifier)
                {
                    return new AssignmentIdentifier(identifier, right);
                }

                if (left is Indexing index)
                {
                    return new AssignmentIndexing(index, right);
                }

                if (left is Member member)
                {
                    return new AssignmentMember(member, right);
                }

                throw new SyntaxErrorException(
                    "Left hand side of assignment must be assignable,",
                    token.Position);

            }

            return left;
        }

        IExpression LogicalOr()
        {
            var left = LogicalAnd();
            while (Match(out var token, TokenTag.Or))
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
            while (Match(out var token, TokenTag.And))
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
            while (Match(out var token, TokenTag.Equals, TokenTag.InequalTo))
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
            while (Match(out var token, TokenTag.GreaterThan, TokenTag.GreaterThanOrEqualTo,
                TokenTag.LessThan, TokenTag.LessThanOrEqualTo))
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
            while (Match(out var token, TokenTag.Add, TokenTag.Subtract))
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
            while (Match(out var token, TokenTag.Multiply, TokenTag.Divide, TokenTag.Mod))
            {
                var op = tokenOp[token.Tag];
                var right = Unary();
                left = new OperatorBinary(token.Position, op, left, right);
            }
            return left;
        }

        IExpression Unary()
        {
            if (Match(out var token, TokenTag.Subtract))
            {
                var expression = Expression();
                return new OperatorUnary(token.Position, OperatorPrefix.Neg, expression);
            }

            if (Match(out token, TokenTag.Not))
            {
                var expression = Expression();
                return new OperatorUnary(token.Position, OperatorPrefix.Not, expression);
            }

            return Invoke();
        }

        IExpression Invoke()
        {
            var expr = Primary();

            while (Match(out var token, TokenTag.LParen, TokenTag.Period,
                TokenTag.LBrace, TokenTag.LBracket))
            {
                if (token.Tag == TokenTag.LParen)
                {
                    expr = Call(token.Position, expr);
                }
                else if (token.Tag == TokenTag.Period)
                {
                    expr = Member(expr);
                }
                else if (token.Tag == TokenTag.LBrace)
                {
                    expr = Constructor(token.Position, expr);
                }
                else if (token.Tag == TokenTag.LBracket)
                {
                    expr = Indexing(expr);
                }
            }

            return expr;
        }

        IExpression Call(Position position, IExpression left)
        {
            List<IExpression> arguments = Arguments();
            if (left is Member member)
            {
                return new MemberCall(position, member, arguments);
            }
            else
            {
                return new Call(position, left, arguments);
            }
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

        IExpression Member(IExpression left)
        {
            var identifierToken = Expect(TokenTag.Identifier);
            var name = new Identifier(identifierToken.Position, identifierToken.Lexeme);
            return new Member(left, name.Name);
        }

        IExpression Constructor(Position position, IExpression left)
        {
            // TODO: This should really be a list to enforce an order.
            var initalizers = new Dictionary<string, IExpression>();

            while (!Match(TokenTag.RBrace))
            {
                var name = Expect(TokenTag.Identifier);
                if (initalizers.ContainsKey(name.Lexeme))
                {
                    throw new SyntaxErrorException(
                        $"Duplicate initializer <{name.Lexeme}>.",
                        name.Position);
                }
                Expect(TokenTag.Colon);
                var value = Expression();
                initalizers.Add(name.Lexeme, value);
            }

            return new RecordConstructor(position, left, initalizers);
        }

        IExpression Indexing(IExpression left)
        {
            var index = Expression();
            Expect(TokenTag.RBracket);
            return new Indexing(left, index);
        }

        IExpression Primary()
        {
            Token token;

            if (Match(out token, TokenTag.Float))
            {
                if (double.TryParse(token.Lexeme, out var value))
                {
                    return new LiteralDouble(value);
                }
                throw new SyntaxErrorException(
                    $"Unable to convert <{token.Lexeme}> into a 64 bit floating bit",
                    token.Position);
            }

            if (Match(out token, TokenTag.Integer))
            {
                if (int.TryParse(token.Lexeme, out var value))
                {
                    return new LiteralInt(value);
                }
                throw new SyntaxErrorException(
                    $"Unable to convert <{token.Lexeme}> into a 32 bit integer.",
                    token.Position);
            }

            if (Match(out token, TokenTag.String))
            {
                return new LiteralString(token.Lexeme);
            }

            if (Match(TokenTag.True))
            {
                return LiteralBool.True;
            }

            if (Match(TokenTag.False))
            {
                return LiteralBool.False;
            }

            if (Match(out token, TokenTag.Identifier))
            {
                return new Identifier(token.Position, token.Lexeme);
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

            if (Match(TokenTag.LBracket))
            {
                return List();
            }

            if (Match(TokenTag.LBrace))
            {
                return Map();
            }

            if (Match(TokenTag.Len))
            {
                return Len();
            }

            if (Match(out token, TokenTag.Push, TokenTag.ReadLn,
                TokenTag.WriteLn))
            {
                return Command(token);
            }

            if (Match(TokenTag.Record))
            {
                return Record();
            }

            throw new SyntaxErrorException($"unexpected token '{current.Tag}'", current.Position);
        }

        IExpression List()
        {
            var initializers = new List<IExpression>();
            if (!Match(TokenTag.RBracket))
            {
                do
                {
                    var initializer = Expression();
                    initializers.Add(initializer);
                }
                while (Match(TokenTag.Comma));
                Expect(TokenTag.RBracket);
            }
            return new List(initializers);
        }

        IExpression Map()
        {
            var initializers = new List<(IExpression, IExpression)>();
            if (!Match(TokenTag.RBrace))
            {
                do
                {
                    var index = Expression();
                    Expect(TokenTag.Colon);
                    var value = Expression();
                    var initializer = (index, value);
                    initializers.Add(initializer);
                }
                while (!Match(TokenTag.RBrace));
            }
            return new Map(initializers);
        }

        IExpression Len()
        {
            Expect(TokenTag.LParen);
            var expression = Expression();
            Expect(TokenTag.RParen);
            return new Len(expression);
        }

        IExpression Command(Token token)
        {
            var commandType = tokenCommand[token.Tag];
            Expect(TokenTag.LParen);
            var arguments = Arguments();
            return new Command(commandType, arguments);
        }

        IExpression Record()
        {
            var variables = new List<string>();
            while (Match(out var idToken, TokenTag.Identifier))
            {
                variables.Add(idToken.Lexeme);
            }

            var functions = new Dictionary<string, Ast.Function>();
            while (Match(TokenTag.Fn))
            {
                var name = Expect(TokenTag.Identifier);
                var parameters = Parameters();
                parameters.Add("this");
                var body = Expression();
                var function = new Function(parameters, body);
                functions.Add(name.Lexeme, function);
            }

            Expect(TokenTag.End);
            return new Ast.Record(variables, functions);
        }
    }
}
