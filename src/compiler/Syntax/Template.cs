namespace BluePrint.Compiler.Syntax;

public sealed record Template : ISyntaxVisitable
{
    public required TypeName TypeName { get; init; }
    public required TypeName? BaseName { get; init; }
    public required IReadOnlyList<ObjectContentElements> Content { get; init; }

    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitTemplate(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        TypeName.Visit(visitor);
        BaseName?.Visit(visitor);

        foreach (var element in Content)
            element.Visit(visitor);
    }
}
