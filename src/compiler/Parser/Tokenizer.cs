using BluePrint.Compiler.Syntax;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BluePrint.Compiler.Parser;

public ref struct Tokenizer()
{
    public required TextStream Stream;

    private Token? _nextToken, _nextToken2;
    public Token Peek()
        => _nextToken ??= ScanNext();

    public Token Peek2()
    {
        _nextToken ??= ScanNext();
        return _nextToken2 ??= ScanNext();
    }

    public Token Next()
    {
        switch ((_nextToken, _nextToken2))
        {
            case ({ } nextToken, { } nextToken2):
                _nextToken = nextToken2;
                _nextToken2 = null;
                return nextToken;

            case ({ } nextToken, null):
                _nextToken = null;
                return nextToken;

            case (null, { }):
                throw new UnreachableException();

            case (null, null):
                return ScanNext();
        }
    }

    private Token ScanNext()
    {
        var c = Stream.Peek();
        while (c != null && char.IsWhiteSpace(c.Value))
        {
            Stream.Advance(n: 1);
            c = Stream.Peek();
        }

        switch (c)
        {
            case null:
                return new Token.EOF();

            case '\"':
            case '\'':
                return ScanStringLiteral();

            case ',':
                Stream.Advance(n: 1);
                return new Token.Comma();

            case ':' when Stream.Peek(offset: 1) is ':':
                Stream.Advance(n: 2);
                return new Token.DoubleColon();

            case ':':
                Stream.Advance(n: 1);
                return new Token.Colon();

            case ';':
                Stream.Advance(n: 1);
                return new Token.Semicolon();

            case '{':
                Stream.Advance(n: 1);
                return new Token.OpenBrace();

            case '}':
                Stream.Advance(n: 1);
                return new Token.CloseBrace();

            case '[':
                Stream.Advance(n: 1);
                return new Token.OpenBracket();

            case ']':
                Stream.Advance(n: 1);
                return new Token.CloseBracket();

            case '(':
                Stream.Advance(n: 1);
                return new Token.OpenParenthesis();

            case ')':
                Stream.Advance(n: 1);
                return new Token.CloseParenthesis();

            case '<':
                Stream.Advance(n: 1);
                return new Token.LessThan();

            case '>':
                Stream.Advance(n: 1);
                return new Token.GreaterThan();

            case '$':
                Stream.Advance(n: 1);
                return new Token.Dollar();

            case '.':
                // TODO: Allow decimals without leading digits?
                Stream.Advance(n: 1);
                return new Token.Dot();

            case '-':
                Stream.Advance(n: 1);
                return new Token.Minus();

            case '+':
                Stream.Advance(n: 1);
                return new Token.Plus();

            case '|':
                Stream.Advance(n: 1);
                return new Token.Pipe();

            case '=' when Stream.Peek(offset: 1) is '>':
                Stream.Advance(n: 2);
                return new Token.LambdaArrow();

            case '_' when Stream.Peek(offset: 1) is '(':
                Stream.Advance(n: 2);
                return new Token.OpenTranslatedString();

            case 'C' when Stream.Peek(offset: 1) is '_' && Stream.Peek(offset: 2) is '(':
                Stream.Advance(n: 3);
                return new Token.OpenContextTranslatedString();

            case '_':
            case (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
                return ScanIdentifierOrKeyword();

            case >= '0' and <= '9':
                return ScanNumericLiteral();

            case '/':
                Stream.Advance(n: 1);
                switch (Stream.Peek())
                {
                    case '/':
                    case '*':
                        throw new NotImplementedException();
                }
                throw new UnexpectedTokenException(actual: c);

            default:
                throw new UnexpectedTokenException(actual: c);
        }
    }

    DefaultInterpolatedStringHandler _builder = new();
    private Token.String ScanStringLiteral()
    {
        _builder.Clear();

        var quoteCharacter = Stream.Pop();
        Debug.Assert(quoteCharacter is '\'' or '"');

        while (true)
        {
            var c = Stream.Peek();
            if (c is '\\')
            {
                c = ScanEscapeSequence(out var c2);
                _builder.AppendFormatted(c.Value);
                if (c2 is not null)
                    _builder.AppendFormatted(c2.Value);
            }
            else if (c == quoteCharacter)
            {
                Stream.Advance(n: 1);
                break;
            }
            else if (c is '\r' or '\n' or null)
                throw new UnexpectedTokenException(actual: c);
            else
            {
                Stream.Advance(n: 1);
                _builder.AppendFormatted(c.Value);
            }
        }

        return new(_builder.ToString());
    }

    private char ScanEscapeSequence(out char? c2)
    {
        var c = Stream.Pop();
        if (c is not '\\')
            throw new UnexpectedTokenException(actual: c);

        c = Stream.Pop();
        c2 = null;
        switch (c)
        {
            // escaped characters that translate to themselves
            case '\'':
            case '"':
            case '\\':
                return c.Value;

            case 'n':
                return '\n';

            case 't':
                return '\t';

            case 'x':
            case 'u':
            case 'U':
                var length = c switch
                {
                    'x' => 2,
                    'u' => 4,
                    'U' => 8,
                    _ => throw new UnreachableException()
                };

                uint uintChar = 0;
                for (int i = 0; i < length; i++)
                {
                    c = Stream.Pop();
                    if (c is null || !SyntaxFacts.IsHexDigit(c.Value))
                        throw new UnexpectedTokenException(actual: c);

                    uintChar = (uint)((uintChar << 4) + SyntaxFacts.HexValue(c.Value));
                }
                return GetCharsFromUtf32(uintChar, out c2);

            default:
                throw new UnexpectedTokenException(actual: c);
        }

        static char GetCharsFromUtf32(uint codepoint, out char? lowSurrogate)
        {
            if (codepoint < (uint)0x00010000)
            {
                lowSurrogate = null;
                return (char)codepoint;
            }
            else
            {
                Debug.Assert(codepoint > 0x0000FFFF && codepoint <= 0x0010FFFF);
                lowSurrogate = (char)((codepoint - 0x00010000) % 0x0400 + 0xDC00);
                return (char)((codepoint - 0x00010000) / 0x0400 + 0xD800);
            }
        }
    }

    private Token ScanIdentifierOrKeyword()
    {
        _builder.Clear();

        var c = Stream.Peek();
        if (c is not ('_' or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z')))
            throw new UnexpectedTokenException(actual: c);

        Stream.Advance(n: 1);
        _builder.AppendFormatted(c);

        while (true)
        {
            c = Stream.Peek();
            if (c is not ('_' or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'))
                break;

            Stream.Advance(n: 1);
            _builder.AppendFormatted(c);
        }

        return new Token.Identifier(_builder.ToString());
    }

    private Token ScanNumericLiteral()
    {
        bool isHex = false;

        var c = Stream.Peek();
        if (c is '0')
        {
            if (Stream.Peek(offset: 1) is 'x' or 'X')
            {
                isHex = true;
                Stream.Advance(n: 2);
            }
        }

        if (isHex)
        {
            int value = 0;
        scanLoop:
            while (true)
            {
                c = Stream.Peek();
                switch (c)
                {
                    case '_':
                        Stream.Advance(n: 1);
                        break;

                    case (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'):
                        Stream.Advance(n: 1);
                        value = (value << 4) + SyntaxFacts.HexValue(c.Value);
                        break;

                    default:
                        break scanLoop;
                }
            }
            return new Token.Number(value);
        }
        else
        {
            bool hasSeenDecimal = false;
            int value = 0;
            int fraction = 0;
            int fractionLength = 0;
        scanLoop:
            for (; ; c = Stream.Peek())
            {
                switch (c)
                {
                    case '_':
                        Stream.Advance(n: 1);
                        break;

                    case >= '0' and <= '9' when !hasSeenDecimal:
                        Stream.Advance(n: 1);
                        value *= 10;
                        value += (c.Value - '0');
                        break;

                    case >= '0' and <= '9' when hasSeenDecimal:
                        Stream.Advance(n: 1);
                        fraction *= 10;
                        fraction += (c.Value - '0');
                        fractionLength++;
                        break;

                    case '.' when hasSeenDecimal:
                        break scanLoop;

                    case '.':
                        Stream.Advance(n: 1);
                        hasSeenDecimal = true;
                        break;

                    default:
                        break scanLoop;
                }
            }

            return new Token.Number(value + (fraction / Math.Pow(10, fractionLength)));
        }
    }
}

public sealed class InvalidTokenException : Exception;
public sealed class UnexpectedTokenException(char? actual) : Exception($"Unexpected token '{actual?.ToString() ?? "EOF"}'");

public closed record Token
{
    public sealed record EOF : Token;
    public sealed record Comma : Token;
    public sealed record Colon : Token;
    public sealed record Semicolon : Token;
    public sealed record OpenBrace : Token;
    public sealed record CloseBrace : Token;
    public sealed record OpenBracket : Token;
    public sealed record CloseBracket : Token;
    public sealed record OpenParenthesis : Token;
    public sealed record CloseParenthesis : Token;
    public sealed record LessThan : Token;
    public sealed record GreaterThan : Token;
    public sealed record Dollar : Token;
    public sealed record Dot : Token;
    public sealed record Minus : Token;
    public sealed record Plus : Token;
    public sealed record Pipe : Token;

    public sealed record DoubleColon : Token;
    public sealed record LambdaArrow : Token;

    public sealed record OpenTranslatedString : Token;
    public sealed record OpenContextTranslatedString : Token;

    public sealed record Identifier(string Name) : Token;
    public sealed record Number(double Value) : Token;
    public sealed record String(string Value) : Token;
}
