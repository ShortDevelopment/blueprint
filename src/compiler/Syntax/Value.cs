using System.Diagnostics;

namespace BluePrint.Compiler.Syntax;

[DebuggerDisplay($"{{{nameof(Value)}}}")]
public readonly union PropertyValue(Menu, ObjectLiteral, Translated, Flags, TypeName, StringLiteral, Number, Bool, Null, Identifier)
{
    public override string ToString() => Value?.ToString() ?? "null";
}
public sealed record Translated(string Value, string? Context);
public sealed record Flags(IReadOnlyList<string> Values);
public sealed record StringLiteral(string Value);
public sealed record Number(double Value);
public sealed record Bool(bool Value);
public sealed record Identifier(string Value);
public sealed record Null;

[DebuggerDisplay($"{{{nameof(Value)}}}")]
public readonly union StringValue(StringLiteral, Translated)
{
    public override string ToString() => Value?.ToString() ?? "null";
}
