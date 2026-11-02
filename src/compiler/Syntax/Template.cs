using BluePrint.Compiler.Parser;

namespace BluePrint.Compiler.Syntax;

public sealed record Template : Element
{
    public required TypeName TypeName { get; init; }
    public required TypeName? BaseName { get; init; }
    public required IReadOnlyList<ObjectContentElements> Content { get; init; }
}
