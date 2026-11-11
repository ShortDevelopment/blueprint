namespace BluePrint.Compiler.Syntax;

public sealed record Menu : ISyntaxVisitable
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }

    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenu(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var child in Children)
            visitor.VisitMenuChild(child);
    }
}
public closed record MenuChild;
public sealed record MenuItem : MenuChild, ISyntaxVisitable
{
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }

    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenuItem(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }
}
public sealed record MenuSection : MenuChild, ISyntaxVisitable
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }

    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenuSection(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var attr in Attributes)
            visitor.VisitMenuAttribute(attr);

        foreach (var child in Children)
            visitor.VisitMenuChild(child);
    }
}
public sealed record MenuSubMenu : MenuChild, ISyntaxVisitable
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }

    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenuSubMenu(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var attr in Attributes)
            visitor.VisitMenuAttribute(attr);

        foreach (var child in Children)
            visitor.VisitMenuChild(child);
    }
}
public sealed record MenuAttribute(string Name, StringValue Value) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenuAttribute(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        Value.Visit(visitor);
    }
}