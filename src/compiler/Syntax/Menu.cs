using BluePrint.Compiler.Parser;

namespace BluePrint.Compiler.Syntax;

public sealed record Menu : Element
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }
}
public closed record MenuChild;
public sealed record MenuItem : MenuChild
{
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
}
public sealed record MenuSection : MenuChild
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }
}
public sealed record MenuSubMenu : MenuChild
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }
}
public sealed record MenuAttribute(string Name, StringValue Value);
