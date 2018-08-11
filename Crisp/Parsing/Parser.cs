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

        public Parser(Scanner scanner, SymbolTable symbolTable) : base(scanner, symbolTable)
        {
        }

        public List<IExpression> Program()
        {
            var expressions = new List<IExpression>();
            while (Current.Tag != TokenTag.EndOfInput)
            {
                var expression = Expression();
                expressions.Add(expression);
            }
            return expressions;
        }

        IExpression Expression()
        {
            if (Match(TokenTag.Var))
            {
                return Var();
            }

            if (Match(TokenTag.Function))
            {
                return Function(isMessageFunction: false);
            }

            if (Match(TokenTag.If))
            {
                return If();
            }

            if (Match(TokenTag.While))
            {
                return While();
            }

            if (Match(TokenTag.Begin))
            {
                return Block();
            }

            if (Match(TokenTag.Type))
            {
                return Type();
            }

            if (Match(TokenTag.Const))
            {
                return Const();
            }

            return Assignment();
        }

        IExpression Var()
        {
            var name = Expect(TokenTag.Identifier);
            CreateSymbol(name, SymbolInfo.Variable());

            Expect(TokenTag.Assignment);
            var initialValue = Expression();
            return new Var(name.Lexeme, initialValue);
        }

        Function Function(bool isMessageFunction)
        {
            var name = Expect(TokenTag.Identifier);
            BeginScope();
            var parameters = Parameters();
            var body = new List<IExpression>();
            while (!Match(TokenTag.End))
            {
                var expr = Expression();
                body.Add(expr);
            }
            EndScope();

            var fn = new Function { Name = name.Lexeme, Parameters = parameters, Body = body };

            if (isMessageFunction)
            {
                CreateSymbol(name, SymbolInfo.MessageFunction());
            }
            else
            {
                CreateSymbol(name, SymbolInfo.Function(fn));
            }

            return fn;
        }

        IExpression If()
        {
            var condition = Expression();
            Expect(TokenTag.Then);
            var consequence = Expression();
            var alternative = Match(TokenTag.Else)
                ? Expression()
                : new Literal { Value = Runtime.Null.Instance };
            return new Branch(condition, consequence, alternative);
        }

        IExpression While()
        {
            var guard = Expression();
            Expect(TokenTag.Do);
            BeginScope();
            var body = new List<IExpression>();
            while (!Match(TokenTag.End))
            {
                var expr = Expression();
                body.Add(expr);
            }
            EndScope();
            return new While(guard, body);
        }

        IExpression Block()
        {
            BeginScope();
            var body = new List<IExpression>();
            while (!Match(TokenTag.End))
            {
                var expr = Expression();
                body.Add(expr);
            }
            EndScope();
            return new Block(body);
        }

        IExpression Type()
        {
            var typeName = Expect(TokenTag.Identifier);
            CreateSymbol(typeName, SymbolInfo.Type());

            // Right now there are only record user defined types.
            Expect(TokenTag.Record);
            BeginScope();

            var variables = new List<string>();
            var functions = new Dictionary<string, Function>();

            if (Current.Tag == TokenTag.Identifier)
            {
                // The short form of a variable field only record.
                variables.AddRange(IdentifierList(SymbolInfo.Attribute()));
            }
            else
            {
                while (true)
                {
                    if (Match(TokenTag.Var))
                    {
                        variables.AddRange(IdentifierList(SymbolInfo.Attribute()));
                    }
                    else if (Match(TokenTag.Function))
                    {
                        var function = Function(isMessageFunction: true);
                        functions.Add(function.Name, function);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Expect(TokenTag.End);
            EndScope();
            return new Record(typeName.Lexeme, variables, functions);
        }

        IExpression Const()
        {
            var name = Expect(TokenTag.Identifier);
            if (SymbolLookup(name) != null)
            {
                throw new SyntaxErrorException($"symbol <{name.Lexeme} is already defined", name.Position);
            }

            Expect(TokenTag.Equals);
            var valuePosition = Current.Position;
            var value = Expression();

            if (value is Literal)
            {
                CreateSymbol(name, SymbolInfo.Constant(value));
                return value;
            }
            else
            {
                throw new SyntaxErrorException($"a constant value must be null or a literal integer, float, Boolean or string", valuePosition);
            }
        }

        List<string> Parameters()
        {

            Expect(TokenTag.LParen);
            if (Match(TokenTag.RParen))
            {
                return new List<string>();
            }
            else
            {
                var parameters = IdentifierList(SymbolInfo.Parameter()).ToList();
                Expect(TokenTag.RParen);
                return parameters;
            }
        }

        IEnumerable<string> IdentifierList(SymbolInfo identifierSymbolInfo)
        {
            do
            {
                var name = Expect(TokenTag.Identifier);
                CreateSymbol(name, identifierSymbolInfo);
                yield return name.Lexeme;
            }
            while (Match(TokenTag.Comma));
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

                if (left is AttributeAccess aa)
                {
                    return new AttributeAssignment(aa.Entity, aa.Name, right);
                }

                throw new SyntaxErrorException("Left hand side of assignment must be assignable,", token.Position);

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
            while (Match(out var token, TokenTag.GreaterThan, TokenTag.GreaterThanOrEqualTo, TokenTag.LessThan, TokenTag.LessThanOrEqualTo))
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
                return new OperatorUnary(token.Position, OperatorUnaryTag.Neg, expression);
            }

            if (Match(out token, TokenTag.Not))
            {
                var expression = Expression();
                return new OperatorUnary(token.Position, OperatorUnaryTag.Not, expression);
            }

            return Invoke();
        }

        IExpression Invoke()
        {
            var expr = Primary();

            while (Match(out var token, TokenTag.LParen, TokenTag.Period, TokenTag.LBrace))
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
            }

            return expr;
        }

        IExpression Call(Position position, IExpression left)
        {
            switch (left)
            {
                case AttributeAccess member:
                    return new MessageSend(position, member.Entity, member.Name, Arguments());

                case Identifier identifier:
                    var siFn = SymbolLookup(identifier.Name);
                    if (siFn?.Tag == SymbolTag.Function)
                    {
                        return new Call { Position = position, Function = (Function)siFn.Value, Arguments = Arguments() };
                    }
                    else
                    {
                        throw new SyntaxErrorException($"no function named <{identifier.Name}> has been defined", identifier.Position);
                    }

                default:
                    throw new SyntaxErrorException("Invalid call target", position);
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
            return new AttributeAccess(left, name.Name);
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
                    throw new SyntaxErrorException($"Duplicate initializer <{name.Lexeme}>.", name.Position);
                }
                Expect(TokenTag.Colon);
                var value = Expression();
                initalizers.Add(name.Lexeme, value);
            }

            return new RecordConstructor(position, left, initalizers);
        }

        IExpression Primary()
        {
            Token token;

            if (Match(out token, TokenTag.Float))
            {
                if (double.TryParse(token.Lexeme, out var value))
                {
                    return new Literal { Value = value };
                }
                throw new SyntaxErrorException($"Unable to convert <{token.Lexeme}> into a 64 bit floating bit", token.Position);
            }

            if (Match(out token, TokenTag.Integer))
            {
                if (int.TryParse(token.Lexeme, out var value))
                {
                    return new Literal { Value = value };
                }
                throw new SyntaxErrorException($"Unable to convert <{token.Lexeme}> into a 32 bit integer.", token.Position);
            }

            if (Match(out token, TokenTag.String))
            {
                var str = ParseString(token.Lexeme, token.Position);
                return new Literal { Value = str };
            }

            if (Match(TokenTag.True))
            {
                return new Literal { Value = true };
            }

            if (Match(TokenTag.False))
            {
                return new Literal { Value = false };
            }

            if (Match(out token, TokenTag.Identifier))
            {
                var name = token.Lexeme;
                var symbolInfo = SymbolLookup(name);
                switch (symbolInfo?.Tag)
                {
                    case SymbolTag.Attribute:
                        return new AttributeAccess(This.Instance, name);
                    case SymbolTag.Constant:
                        return symbolInfo.Value;
                    default:
                        return new Identifier(token.Position, name);
                }
            }

            if (Match(TokenTag.Null))
            {
                return new Literal { Value = Runtime.Null.Instance };
            }

            if (Match(TokenTag.This))
            {
                return This.Instance;
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
            var arguments = Arguments();
            return new Write { Arguments = arguments };
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
