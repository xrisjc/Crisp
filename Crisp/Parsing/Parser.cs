using System.Collections.Generic;

namespace Crisp.Parsing
{
    class Parser
    {
        readonly Lexer lexer;
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
                [TokenTag.Modulo] = Precedence.Multiplicitive,
                [TokenTag.Multiply] = Precedence.Multiplicitive,

                [TokenTag.LBracket] = Precedence.Index,

                [TokenTag.LParen] = Precedence.Parentheses,
            };


        static Dictionary<TokenTag, IOperatorBinary> biOp =
            new Dictionary<TokenTag, IOperatorBinary>
            {
                [TokenTag.Add] = OperatorAdd.Instance,
                [TokenTag.Divide] = OperatorDivide.Instance,
                [TokenTag.Equals] = OperatorEquals.Instance,
                [TokenTag.GreaterThan] = OperatorGreaterThan.Instance,
                [TokenTag.GreaterThanOrEqualTo] = OperatorGreaterThanOrEqualTo.Instance,
                [TokenTag.InequalTo] = OperatorInequalTo.Instance,
                [TokenTag.LessThan] = OperatorLessThan.Instance,
                [TokenTag.LessThanOrEqualTo] = OperatorLessThanOrEqualTo.Instance,
                [TokenTag.Modulo] = OperatorModulo.Instance,
                [TokenTag.Multiply] = OperatorMultiply.Instance,
                [TokenTag.Subtract] = OperatorSubtract.Instance,
            };

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            NextToken();
            NextToken();
        }

        void NextToken()
        {
            current = peek;
            peek = lexer.NextToken();
        }

        public bool IsFinished => current.Tag == TokenTag.EndOfInput;

        bool Match(TokenTag tag)
        {
            var isMatch = current.Tag == tag;
            if (isMatch)
            {
                NextToken();
            }
            return isMatch;
        }

        bool MatchValue<T>(TokenTag tag, out TokenValue<T> tokenValue)
        {
            if (current.Tag == tag && current is TokenValue<T> tv)
            {
                tokenValue = tv;
                NextToken();
                return true;
            }
            else
            {
                tokenValue = null;
                return false;
            }
        }

        void Expect(TokenTag tag)
        {
            if (!Match(tag))
            {
                throw new SyntaxErrorException(
                    $"exepected, but didn't match, token {tag} at position {current.Position}");
            }
        }

        TokenValue<T> ExpectValue<T>(TokenTag tag)
        {
            if (current.Tag == tag && current is TokenValue<T> tokenValue)
            {
                NextToken();
                return tokenValue;
            }
            else
            {
                throw new SyntaxErrorException(
                    $"exepected, but didn't match, token {tag} at position {current.Position}");
            }
        }

        Precedence Lbp(Token token)
        {
            if (precedences.TryGetValue(token.Tag, out var precedence))
            {
                return precedence;
            }
            else
            {
                return Precedence.Lowest;
            }
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
                            : ExpressionLiteralNull.Instance;
                        return new ExpressionBranch(
                            condition,
                            consequence,
                            alternative);
                    }

                case TokenTag.While:
                    {
                        var guard = ParseExpression();
                        Expect(TokenTag.Do);
                        var body = ParseExpression();
                        return new ExpressionWhile(guard, body);
                    }

                case TokenTag.Let:
                    {
                        var identifier = ExpectValue<string>(TokenTag.Identifier);
                        Expect(TokenTag.Assignment);
                        var value = ParseExpression(Precedence.Assignment);
                        return new ExpressionLet(identifier.Value, value);
                    }

                case TokenTag.Begin:
                    {
                        var body = new List<IExpression>();
                        while (!Match(TokenTag.End))
                        {
                            body.Add(ParseExpression());
                        }
                        return new ExpressionBlock(body);
                    }

                case TokenTag.Fn:
                    {
                        var parameters = new List<string>();
                        string name = MatchValue<string>(TokenTag.Identifier, out var nameToken)
                            ? nameToken.Value
                            : null;
                        Expect(TokenTag.LParen);
                        if (MatchValue<string>(TokenTag.Identifier, out var identifier))
                        {
                            parameters.Add(identifier.Value);
                            while (Match(TokenTag.Comma))
                            {
                                identifier = ExpectValue<string>(TokenTag.Identifier);
                                parameters.Add(identifier.Value);
                            }
                        }
                        Expect(TokenTag.RParen);
                        var body = ParseExpression();
                        return new ExpressionFunction(name, parameters, body);
                    }

                case TokenTag.LBrace when Match(TokenTag.RBrace):
                        return new ExpressionMap();

                case TokenTag.LBrace:
                    {
                        var initializers = new List<IndexValuePair>();
                        do
                        {
                            Expect(TokenTag.LBracket);
                            var index = ParseExpression();
                            Expect(TokenTag.RBracket);
                            Expect(TokenTag.Equals);
                            var value = ParseExpression();
                            var initializer = new IndexValuePair(index, value);
                            initializers.Add(initializer);

                        } while (Match(TokenTag.Comma));
                        Expect(TokenTag.RBrace);
                        return new ExpressionMap(initializers);
                    }

                case TokenTag.LParen:
                    {
                        var expression = ParseExpression();
                        Expect(TokenTag.RParen);
                        return expression;
                    }

                case TokenTag.Identifier when token is TokenValue<string> tokenValue:
                    return new ExpressionIdentifier(tokenValue.Value);

                case TokenTag.String when token is TokenValue<string> tokenValue:
                    return new ExpressionLiteral<string>(tokenValue.Value);

                case TokenTag.Number when token is TokenValue<double> tokenValue:
                    return new ExpressionLiteral<double>(tokenValue.Value);

                case TokenTag.False:
                    return new ExpressionLiteral<bool>(false);

                case TokenTag.True:
                    return new ExpressionLiteral<bool>(true);

                case TokenTag.Null:
                    return ExpressionLiteralNull.Instance;

                default:
                    throw new SyntaxErrorException(
                        $"unexpected token '{token.Tag}' at {token.Position}");
            }
        }

        IExpression Led(Token token, IExpression left)
        {
            switch (token.Tag)
            {
                case TokenTag.Assignment when left is ExpressionIdentifier identifier:
                    return new ExpressionAssignmentVariable(
                        identifier,
                        ParseExpression(Lbp(token)));

                case TokenTag.Assignment when left is ExpressionIndex index:
                    return new ExpressionAssignmentIndex(
                        index,
                        ParseExpression(Lbp(token)));

                case TokenTag.Assignment:
                    throw new SyntaxErrorException(
                        "left hand side of assignment must be assignable");

                case TokenTag.And:
                    return new ExpressionLogicalAnd(
                        left: left,
                        right: ParseExpression(Lbp(token)));

                case TokenTag.Or:
                    return new ExpressionLogicalOr(
                        left: left,
                        right: ParseExpression(Lbp(token)));

                case TokenTag.Add:
                case TokenTag.Equals:
                case TokenTag.Divide:
                case TokenTag.GreaterThan:
                case TokenTag.GreaterThanOrEqualTo:
                case TokenTag.InequalTo:
                case TokenTag.LessThan:
                case TokenTag.LessThanOrEqualTo:
                case TokenTag.Modulo:
                case TokenTag.Multiply:
                case TokenTag.Subtract:
                    return new ExpressionOperatorBinary(
                        biOp[token.Tag],
                        left,
                        ParseExpression(Lbp(token)));

                case TokenTag.LBracket:
                    {
                        var index = ParseExpression();
                        Expect(TokenTag.RBracket);
                        return new ExpressionIndex(left, index);
                    }

                case TokenTag.LParen when Match(TokenTag.RParen):
                    return new ExpressionCall(left);

                case TokenTag.LParen:
                    {
                        var argumentExpressions = new List<IExpression>();
                        do
                        {
                            argumentExpressions.Add(ParseExpression());
                        } while (Match(TokenTag.Comma));
                        Expect(TokenTag.RParen);
                        return new ExpressionCall(left, argumentExpressions);
                    }


                default:
                    throw new SyntaxErrorException(
                        $"unexpected token '{token.Tag}' at {token.Position}");
            }
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
