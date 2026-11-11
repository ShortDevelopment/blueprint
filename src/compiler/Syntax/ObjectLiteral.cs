namespace BluePrint.Compiler.Syntax;

public sealed record ObjectLiteral : ISyntaxVisitable
{
    public required TypeName TypeName { get; init; }
    public required string? Id { get; init; }
    public required IReadOnlyList<ObjectContentElements> Content { get; init; }

    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitObject(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        TypeName.Visit(visitor);

        foreach (var element in Content)
            element.Visit(visitor);
    }
}

public readonly union ObjectContentElements(Child, Property, Signal) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).Visit(visitor);

    public void VisitChildren(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).VisitChildren(visitor);
}

public sealed record Child(ChildAnnotation? Annotation, ObjectLiteral Value) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitChild(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        visitor.VisitObject(Value);
    }
}

public sealed record ChildAnnotation(bool IsInternal, string ChildType);

public sealed record Property(string Name, PropertyValue Value) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitProperty(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        Value.Visit(visitor);
    }
}

public sealed record Signal(string Name, string? Detail, string Handler, string? Object, SignalFlags Flags) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitSignal(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }
}

[Flags]
public enum SignalFlags
{
    After = 1 << 1,
    Swapped = 1 << 2,
    NotSwapped = 1 << 3,
}
