using Crisp.Ast;
using Crisp.Eval;
using System.Collections.Generic;

namespace Crisp.Parsing
{
    class Parser
    {
        readonly Scanner scanner;
        Token current = null;
        Token peek = null;

        static Dictionary<TokenTag, Precedence> precedences =
            new Dictionary<TokenTag, Precedence>
            {
                [TokenTag.Assignment] = Precedence.Assignment,

                [TokenTag.Or] = Precedence.LogicalOr,

                [TokenTag.And] = Precedence.LogicalAnd,

                [TokenTag.Equals] = Precedence.Equality,
                [TokenTag.InequalTo] = Precedence.Equality,


                [TokenTag.GreaterThan] = Precedence.Relational,
                [TokenTag.GreaterThanOrEqualTo] = Precedence.Relational,
                [TokenTag.LessThan] = Precedence.Relational,
                [TokenTag.LessThanOrEqualTo] = Precedence.Relational,

                [TokenTag.Add] = Precedence.Additive,
                [TokenTag.Subtract] = Precedence.Additive,

                [TokenTag.Divide] = Precedence.Multiplicitive,
                [TokenTag.Mod] = Precedence.Multiplicitive,
                [TokenTag.Multiply] = Precedence.Multiplicitive,

                [TokenTag.LBrace] = Precedence.Expression,
                [TokenTag.LBracket] = Precedence.Expression,
                [TokenTag.LParen] = Precedence.Expression,
                [TokenTag.Period] = Precedence.Expression,
            };

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

        public bool IsFinished => current.Tag == TokenTag.EndOfInput;

        bool Match(TokenTag tag, out Token token)
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
            return Match(tag, out var token);
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
                    $"exepected, but didn't match, token {tag} at position {current.Position}");
            }
        }

        Precedence Lbp(Token token)
        {
            return precedences.GetValue(token.Tag, Precedence.Lowest);
        }

        IExpression Nud(Token token)
        {
            switch (token.Tag)
            {
                case TokenTag.If:
                    {
                        var condition = ParseExpression();
                        Expect(TokenTag.Then);
                        var consequence = ParseExpression();
                        var alternative = Match(TokenTag.Else)
                            ? ParseExpression()
                            : LiteralNull.Instance;
                        return new Branch(condition, consequence, alternative);
                    }

                case TokenTag.While:
                    {
                        var guard = ParseExpression();
                        Expect(TokenTag.Do);
                        var body = ParseExpression();
                        return new While(guard, body);
                    }

                case TokenTag.Let:
                    {
                        var identifier = ParseIdentifier();
                        Expect(TokenTag.Assignment);
                        var value = ParseExpression(Precedence.Assignment);
                        return new Let(identifier, value);
                    }

                case TokenTag.Begin:
                    {
                        var body = new List<IExpression>();
                        while (!Match(TokenTag.End))
                        {
                            body.Add(ParseExpression());
                        }
                        return new Block(body);
                    }

                case TokenTag.Fn:
                    {
                        // Function name
                        var hasName = Match(TokenTag.Identifier, out var nameToken);

                        // Function parameter list
                        Expect(TokenTag.LParen);
                        var parameters = new List<Identifier>();
                        if (!Match(TokenTag.RParen))
                        {
                            do
                            {
                                var identifier = ParseIdentifier();
                                parameters.Add(identifier);
                            }
                            while (Match(TokenTag.Comma));
                            Expect(TokenTag.RParen);
                        }

                        // Function body
                        var body = ParseExpression();

                        // Create AST
                        if (hasName)
                        {
                            var name = new Identifier(nameToken.Lexeme);
                            return new NamedFunction(name, parameters, body);
                        }
                        else
                        {
                            return new Function(parameters, body);
                        }
                    }

                case TokenTag.LBrace when Match(TokenTag.RBrace):
                    return new Map();

                case TokenTag.LBrace:
                    {
                        var initializers = new List<(IExpression, IExpression)>();
                        do
                        {
                            var index = ParseExpression();
                            Expect(TokenTag.Colon);
                            var value = ParseExpression();
                            var initializer = (index, value);
                            initializers.Add(initializer);

                        } while (!Match(TokenTag.RBrace));
                        return new Map(initializers);
                    }

                case TokenTag.LParen:
                    {
                        var expression = ParseExpression();
                        Expect(TokenTag.RParen);
                        return expression;
                    }

                case TokenTag.Identifier:
                    return new Identifier(token.Lexeme);

                case TokenTag.Integer
                when int.TryParse(token.Lexeme, out int value):
                    return new Literal<int>(value);

                case TokenTag.Integer:
                    throw new SyntaxErrorException(
                        $"Unable to convert <{token.Lexeme}> into an 32 bit integer.");

                case TokenTag.Record when Match(TokenTag.End):
                    return new Record();

                case TokenTag.Record:
                    {
                        var members = new List<Identifier>();
                        while (Match(TokenTag.Identifier, out var idToken))
                        {
                            var id = new Identifier(idToken.Lexeme);
                            members.Add(id);
                        }
                        Expect(TokenTag.End);
                        return new Record(members);
                    }

                case TokenTag.String:
                    return new Literal<string>(token.Lexeme);

                case TokenTag.Subtract:
                    return new OperatorUnary(
                        OperatorPrefix.Neg,
                        ParseExpression());

                case TokenTag.Float
                when double.TryParse(token.Lexeme, out var value):
                    return new Literal<double>(value);

                case TokenTag.Float:
                    throw new SyntaxErrorException(
                        $"Unable to convert <{token.Lexeme}> into a 64 bit floating bit");

                case TokenTag.False:
                    return new Literal<bool>(false);

                case TokenTag.True:
                    return new Literal<bool>(true);

                case TokenTag.Not:
                    return new OperatorUnary(
                        OperatorPrefix.Not,
                        ParseExpression());

                case TokenTag.Null:
                    return LiteralNull.Instance;

                default:
                    throw new SyntaxErrorException(
                        $"unexpected token '{token.Tag}' at {token.Position}");
            }
        }

        IExpression Led(Token token, IExpression left)
        {
            switch (token.Tag)
            {
                case TokenTag.Assignment when left is Identifier identifier:
                    return new Assignment<Identifier>(identifier, ParseExpression(Lbp(token)));

                case TokenTag.Assignment when left is Indexing index:
                    return new Assignment<Indexing>(index, ParseExpression(Lbp(token)));

                case TokenTag.Assignment when left is Member member:
                    return new Assignment<Member>(member, ParseExpression(Lbp(token)));

                case TokenTag.Assignment:
                    throw new SyntaxErrorException(
                        "left hand side of assignment must be assignable");

                case TokenTag.Add:
                case TokenTag.And:
                case TokenTag.Divide:
                case TokenTag.Equals:
                case TokenTag.GreaterThan:
                case TokenTag.GreaterThanOrEqualTo:
                case TokenTag.InequalTo:
                case TokenTag.LessThan:
                case TokenTag.LessThanOrEqualTo:
                case TokenTag.Mod:
                case TokenTag.Multiply:
                case TokenTag.Or:
                case TokenTag.Subtract:
                    return new OperatorBinary(
                        tokenOp[token.Tag],
                        left,
                        ParseExpression(Lbp(token)));

                case TokenTag.LBrace when Match(TokenTag.RBrace):
                    return new RecordConstructor(left);

                case TokenTag.LBrace:
                    {
                        var initalizers = new List<(Identifier, IExpression)>();
                        do
                        {
                            Identifier id = ParseIdentifier();
                            Expect(TokenTag.Colon);
                            var value = ParseExpression();
                            var initalizer = (id, value);
                            initalizers.Add(initalizer);
                        }
                        while (!Match(TokenTag.RBrace));
                        return new RecordConstructor(left, initalizers);
                    }

                case TokenTag.LBracket:
                    {
                        var index = ParseExpression();
                        Expect(TokenTag.RBracket);
                        return new Indexing(left, index);
                    }

                case TokenTag.LParen when Match(TokenTag.RParen):
                    return new Call(left);

                case TokenTag.LParen:
                    {
                        var argumentExpressions = new List<IExpression>();
                        do
                        {
                            argumentExpressions.Add(ParseExpression());
                        } while (Match(TokenTag.Comma));
                        Expect(TokenTag.RParen);
                        return new Call(left, argumentExpressions);
                    }

                case TokenTag.Period:
                    {
                        var identifier = ParseIdentifier();
                        return new Member(left, identifier);
                    }


                default:
                    throw new SyntaxErrorException(
                        $"unexpected token '{token.Tag}' at {token.Position}");
            }
        }

        Identifier ParseIdentifier()
        {
            var idToken = Expect(TokenTag.Identifier);
            return new Identifier(idToken.Lexeme);
        }

        public IExpression ParseExpression(Precedence rbp = Precedence.Lowest)
        {
            var t = current;
            NextToken();
            var left = Nud(t);
            while (rbp < Lbp(current))
            {
                t = current;
                NextToken();
                left = Led(t, left);
            }
            return left;
        }

    }
}
