using System.Diagnostics;

namespace BluePrint.Compiler.Syntax;

/// <summary>
/// Defines a set of methods to determine how Unicode characters are treated by the Blueprint compiler.
/// </summary>
internal static class SyntaxFacts
{
    /// <summary>
    /// Returns true if the Unicode character is a hexadecimal digit.
    /// </summary>
    /// <param name="c">The Unicode character.</param>
    /// <returns>true if the character is a hexadecimal digit 0-9, A-F, a-f.</returns>
    internal static bool IsHexDigit(char c)
    {
        return (c >= '0' && c <= '9') ||
               (c >= 'A' && c <= 'F') ||
               (c >= 'a' && c <= 'f');
    }

    /// <summary>
    /// Returns the value of a hexadecimal Unicode character.
    /// </summary>
    /// <param name="c">The Unicode character.</param>
    internal static int HexValue(char c)
    {
        Debug.Assert(IsHexDigit(c));
        return (c >= '0' && c <= '9') ? c - '0' : (c & 0xdf) - 'A' + 10;
    }

    /// <summary>
    /// Returns true if the Unicode character represents a whitespace.
    /// </summary>
    /// <param name="ch">The Unicode character.</param>
    internal static bool IsWhiteSpace(char c)
        => char.IsWhiteSpace(c);

    /// <summary>
    /// Returns true if the Unicode character is a newline character.
    /// </summary>
    /// <param name="c">The Unicode character.</param>
    internal static bool IsNewLine(char c) => c is '\r' or '\n';
}
