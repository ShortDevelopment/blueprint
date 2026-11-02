namespace BluePrint.Compiler.Syntax;

public sealed record ObjectLiteral : Element
{
    public required TypeName TypeName { get; init; }
    public required string? Id { get; init; }
    public required IReadOnlyList<ObjectContentElements> Content { get; init; }
}

public closed record ObjectContentElements;

public sealed record Child(ChildAnnotation? Annotation, ObjectLiteral Value) : ObjectContentElements;
public sealed record ChildAnnotation(bool IsInternal, string ChildType);

public sealed record Property(string Name, PropertyValue Value) : ObjectContentElements;

public sealed record Signal(string Name, string? Detail, string Handler, string? Object, SignalFlags Flags) : ObjectContentElements;

[Flags]
public enum SignalFlags
{
    After = 1 << 1,
    Swapped = 1 << 2,
    NotSwapped = 1 << 3,
}
