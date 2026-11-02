using BluePrint.Compiler;
using TUnit.Assertions.Enums;

namespace Blueprint.Compiler.Tests;

public class TokenizerTests
{
    [Test]
    public async Task Peek_ShouldReturnFirstToken()
    {
        var source = $"""
        1 2 3 4
        """;

        var tokenizer = Tokenizer(source);

        var peek = tokenizer.Peek();
        var next = tokenizer.Next();

        await Assert.That(peek).IsSameReferenceAs(next);
        await Assert.That(peek).IsEqualTo(new Token.Number(1));
    }

    [Test]
    public async Task Peek_ShouldBeIdempotent()
    {
        var source = $"""
        1 2 3 4
        """;

        var tokenizer = Tokenizer(source);

        var peek1 = tokenizer.Peek();
        var peek2 = tokenizer.Peek();
        var peek3 = tokenizer.Peek();
        var next = tokenizer.Next();

        await Assert.That(peek1)
            .IsSameReferenceAs(peek2).And
            .IsSameReferenceAs(peek3).And
            .IsSameReferenceAs(next);
        await Assert.That(peek1).IsEqualTo(new Token.Number(1));
    }

    [Test]
    public async Task Peek2_ShouldReturnSecondToken()
    {
        var source = $"""
        1 2 3 4
        """;

        var tokenizer = Tokenizer(source);

        var peek = tokenizer.Peek2();
        var next1 = tokenizer.Next();
        var next2 = tokenizer.Next();

        await Assert.That(peek).IsSameReferenceAs(next2);
        await Assert.That(peek).IsEqualTo(new Token.Number(2));

        await Assert.That(next1).IsEqualTo(new Token.Number(1));
        await Assert.That(next2).IsEqualTo(new Token.Number(2));
    }

    [Test]
    public async Task Peek2_ShouldBeIdempotent()
    {
        var source = $"""
        1 2 3 4
        """;

        var tokenizer = Tokenizer(source);

        var peek1 = tokenizer.Peek2();
        var peek2 = tokenizer.Peek2();
        var peek3 = tokenizer.Peek2();

        var next1 = tokenizer.Next();
        var next2 = tokenizer.Next();

        await Assert.That(peek1)
            .IsSameReferenceAs(peek2).And
            .IsSameReferenceAs(peek3).And
            .IsSameReferenceAs(next2);

        await Assert.That(next1).IsEqualTo(new Token.Number(1));
        await Assert.That(peek1).IsEqualTo(new Token.Number(2));
    }

    [Test]
    public async Task Peek_ShouldNotInfluencePeek2()
    {
        var source = $"""
        1 2 3 4
        """;

        var tokenizer = Tokenizer(source);

        var peek1_1 = tokenizer.Peek();
        var peek1_2 = tokenizer.Peek();
        var peek2_1 = tokenizer.Peek2();
        var peek2_2 = tokenizer.Peek2();

        var next1 = tokenizer.Next();
        var next2 = tokenizer.Next();

        await Assert.That(next1).IsEqualTo(new Token.Number(1));
        await Assert.That(peek1_1)
            .IsSameReferenceAs(peek1_2).And
            .IsSameReferenceAs(next1);

        await Assert.That(next2).IsEqualTo(new Token.Number(2));
        await Assert.That(peek2_1)
            .IsSameReferenceAs(peek2_2).And
            .IsSameReferenceAs(next2);
    }

    [Test]
    public async Task Peek2_ShouldNotInfluencePeek1()
    {
        var source = $"""
        1 2 3 4
        """;

        var tokenizer = Tokenizer(source);

        var peek2_1 = tokenizer.Peek2();
        var peek2_2 = tokenizer.Peek2();
        var peek1_1 = tokenizer.Peek();
        var peek1_2 = tokenizer.Peek();

        var next1 = tokenizer.Next();
        var next2 = tokenizer.Next();

        await Assert.That(next1).IsEqualTo(new Token.Number(1));
        await Assert.That(peek1_1)
            .IsSameReferenceAs(peek1_2).And
            .IsSameReferenceAs(next1);

        await Assert.That(next2).IsEqualTo(new Token.Number(2));
        await Assert.That(peek2_1)
            .IsSameReferenceAs(peek2_2).And
            .IsSameReferenceAs(next2);
    }

    [Test]
    public async Task SymbolTokens()
        => await AssertTokenize(
            ", : :: ; { } [ ] ( ) < > $ . - + | => _( C_( 42",
            new Token.Comma(),
            new Token.Colon(),
            new Token.DoubleColon(),
            new Token.Semicolon(),
            new Token.OpenBrace(),
            new Token.CloseBrace(),
            new Token.OpenBracket(),
            new Token.CloseBracket(),
            new Token.OpenParenthesis(),
            new Token.CloseParenthesis(),
            new Token.LessThan(),
            new Token.GreaterThan(),
            new Token.Dollar(),
            new Token.Dot(),
            new Token.Minus(),
            new Token.Plus(),
            new Token.Pipe(),
            new Token.LambdaArrow(),
            new Token.OpenTranslatedString(),
            new Token.OpenContextTranslatedString(),
            new Token.Number(42)
        );

    [Test]
    public async Task SymbolTokens_NoSpace()
    => await AssertTokenize(
        ",:::;{}[]()<>$.-+|=>_(C_(42",
        new Token.Comma(),
        new Token.DoubleColon(),
        new Token.Colon(),
        new Token.Semicolon(),
        new Token.OpenBrace(),
        new Token.CloseBrace(),
        new Token.OpenBracket(),
        new Token.CloseBracket(),
        new Token.OpenParenthesis(),
        new Token.CloseParenthesis(),
        new Token.LessThan(),
        new Token.GreaterThan(),
        new Token.Dollar(),
        new Token.Dot(),
        new Token.Minus(),
        new Token.Plus(),
        new Token.Pipe(),
        new Token.LambdaArrow(),
        new Token.OpenTranslatedString(),
        new Token.OpenContextTranslatedString(),
        new Token.Number(42)
    );

    [Test]
    public async Task IntegerSingleDigit()
        => await AssertTokenize(
            $"""0 1 2 3 4 5 6 7 8 9""",
            new Token.Number(0),
            new Token.Number(1),
            new Token.Number(2),
            new Token.Number(3),
            new Token.Number(4),
            new Token.Number(5),
            new Token.Number(6),
            new Token.Number(7),
            new Token.Number(8),
            new Token.Number(9)
        );

    [Test]
    public async Task IntegerSingleDigit_AllowTrailingDot()
        => await AssertTokenize(
            $"""1. 2 3. 4 5""",
            new Token.Number(1),
            new Token.Number(2),
            new Token.Number(3),
            new Token.Number(4),
            new Token.Number(5)
        );

    // TODO: We currently lexing leading dots before numbers as separate tokens

    [Test]
    public async Task IntegerSingleDigit_LeadingDotIsSeparate()
        => await AssertTokenize(
            $"""1 .2 3 .4 5""",
            new Token.Number(1),
            new Token.Dot(),
            new Token.Number(2),
            new Token.Number(3),
            new Token.Dot(),
            new Token.Number(4),
            new Token.Number(5)
        );

    [Test]
    public async Task IntegerSingleDigit_SeparateDotIsSeparate()
        => await AssertTokenize(
            $"""1 . 2 3 . 4 5""",
            new Token.Number(1),
            new Token.Dot(),
            new Token.Number(2),
            new Token.Number(3),
            new Token.Dot(),
            new Token.Number(4),
            new Token.Number(5)
        );

    [Test]
    public async Task IntegerMultipleDots()
        => await AssertTokenize(
            $"""1..2 3..4 5""",
            new Token.Number(1),
            new Token.Dot(),
            new Token.Number(2),
            new Token.Number(3),
            new Token.Dot(),
            new Token.Number(4),
            new Token.Number(5)
        );

    [Test]
    public async Task Double()
        => await AssertTokenize(
            $"""1.2 3.4 5""",
            new Token.Number(1.2),
            new Token.Number(3.4),
            new Token.Number(5)
        );

    [Test]
    public async Task HexSingleDigit()
        => await AssertTokenize(
            $"""0x0 0x1 0x2 0x3 0x4 0x5 0x6 0x7 0x8 0x9 0xa 0xb 0xc 0xd 0xe 0xf 0xA 0xB 0xC 0xD 0xE 0xF""",
            new Token.Number(0x0),
            new Token.Number(0x1),
            new Token.Number(0x2),
            new Token.Number(0x3),
            new Token.Number(0x4),
            new Token.Number(0x5),
            new Token.Number(0x6),
            new Token.Number(0x7),
            new Token.Number(0x8),
            new Token.Number(0x9),
            new Token.Number(0xa),
            new Token.Number(0xb),
            new Token.Number(0xc),
            new Token.Number(0xd),
            new Token.Number(0xe),
            new Token.Number(0xf),
            new Token.Number(0xA),
            new Token.Number(0xB),
            new Token.Number(0xC),
            new Token.Number(0xD),
            new Token.Number(0xE),
            new Token.Number(0xF)
        );

    // TODO: We currently allow trailing underscores in integers / doubles

    [Test]
    public async Task Integer_AllowUnderscore()
        => await AssertTokenize(
            $"""1__ 1_0 1__000 10_000""",
            new Token.Number(1),
            new Token.Number(1_0),
            new Token.Number(1__000),
            new Token.Number(10_000)
        );

    // TODO: We currently allow trailing underscores in hex numbers

    [Test]
    public async Task Hex_AllowUnderscore()
        => await AssertTokenize(
            $"""0x0_ 0x_0 0x1_00 0x12_345""",
            new Token.Number(0x0),
            new Token.Number(0x_0),
            new Token.Number(0x1_00),
            new Token.Number(0x12_345)
        );

    [Test]
    public async Task Identifier_LeadingUnderscoreIsIdentifier()
        => await AssertTokenize(
            $"""_ _a _1 _abc _123""",
            new Token.Identifier("_"),
            new Token.Identifier("_a"),
            new Token.Identifier("_1"),
            new Token.Identifier("_abc"),
            new Token.Identifier("_123")
        );

    private static async Task AssertTokenize(string source, params Token[] expectedTokens)
    {
        var tokenizer = Tokenizer(source);

        List<Token> actualTokens = [];
        for (int i = 0; i < expectedTokens.Length; i++)
            actualTokens.Add(tokenizer.Next());
        var end = tokenizer.Next();

        await Assert.That(end).IsEqualTo(new Token.EOF());
        await Assert.That(actualTokens).IsEquivalentTo(expectedTokens, CollectionOrdering.Matching);
    }

    private static Tokenizer Tokenizer(ReadOnlySpan<char> source)
        => new() { Stream = new(source) };
}
