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
            child.Visit(visitor);
    }
}
public closed record MenuChild : ISyntaxVisitable
{
    public abstract void Visit(ISyntaxVisitor visitor);
    public abstract void VisitChildren(ISyntaxVisitor visitor);
}

public sealed record MenuItem : MenuChild
{
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }

    public override void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenuItem(this);

    public override void VisitChildren(ISyntaxVisitor visitor) { }
}
public sealed record MenuSection : MenuChild
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }

    public override void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenuSection(this);

    public override void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var attr in Attributes)
            attr.Visit(visitor);

        foreach (var child in Children)
            child.Visit(visitor);
    }
}
public sealed record MenuSubMenu : MenuChild
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }

    public override void Visit(ISyntaxVisitor visitor)
        => visitor.VisitMenuSubMenu(this);

    public override void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var attr in Attributes)
            attr.Visit(visitor);

        foreach (var child in Children)
            child.Visit(visitor);
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