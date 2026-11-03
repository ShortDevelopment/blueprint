using BluePrint.Compiler.Syntax;

namespace BluePrint.Compiler.Parser;

partial struct Parser
{
    private Expression Expression()
    {
        Expression result = ExpressionAtom();

    parseLoop:
        while (true)
        {
            switch (Tokenizer.Peek())
            {
                case Token.Dot:
                    Expect<Token.Dot>();
                    var property = Expect<Token.Identifier>().Name;

                    result = new LookupExpression(result, property);
                    break;

                case Token.Identifier { Name: "as" }:
                    Expect<Token.Identifier>(static x => x.Name == "as");

                    Expect<Token.LessThan>();
                    var typeName = TypeName();
                    Expect<Token.GreaterThan>();

                    result = new CastExpression(result, typeName);
                    break;

                default:
                    break parseLoop;
            }
        }

        return result;
    }

    private Expression ExpressionAtom()
    {
        switch (Tokenizer.Peek())
        {
            case Token.OpenTranslatedString:
            case Token.OpenContextTranslatedString:
                return Translated();

            case Token.Identifier { Name: "try" }:
                return TryExpression();

            case Token.Dollar:
                return ClosureExpression();

            // == Literal == //

            case Token.Identifier { Name: "typeof" }:
                return TypeLiteral();

            case Token.String stringToken:
                Expect<Token.String>();
                return new StringLiteral(stringToken.Value);

            case Token.Minus:
            case Token.Plus:
            case Token.Number:
                return NumberLiteral();

            case Token.Identifier idToken:
                Expect<Token.Identifier>();
                return new Identifier(idToken.Name);

            // == Nested expressions == //

            case Token.OpenParenthesis:
                Expect<Token.OpenParenthesis>();
                var subExpression = Expression();
                Expect<Token.CloseParenthesis>();
                return subExpression;

            default:
                throw new SyntaxError($"Invalid expression atom");
        }
    }

    private ClosureExpression ClosureExpression()
    {
        Expect<Token.Dollar>();
        var name = Expect<Token.Identifier>().Name;
        Expect<Token.OpenParenthesis>();

        List<Expression> args = [];
        do
        {
            args.Add(Expression());
        } while (TryTake<Token.Comma>() is { });

        Expect<Token.CloseParenthesis>();

        return new(name, args);
    }

    private TryExpression TryExpression()
    {
        Expect<Token.Identifier>(static x => x.Name == "try");

        Expect<Token.OpenBrace>();

        List<Expression> expressions = [];
        do
        {
            expressions.Add(Expression());
        } while (TryTake<Token.Comma>() is { });

        Expect<Token.CloseBrace>();

        return new(expressions);
    }
}
