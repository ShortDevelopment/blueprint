namespace BluePrint.Compiler.Syntax;

public sealed record Document : ISyntaxVisitable
{
    public required IReadOnlyList<Using> Usings { get; init; }
    public required string? TranslationDomain { get; init; }
    public required IReadOnlyList<Element> Elements { get; init; }

    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitDocument(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var @using in Usings)
            @using.Visit(visitor);

        foreach (var element in Elements)
            element.Visit(visitor);
    }
}

public sealed record Using(string Namespace, double Version) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitUsing(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }
}

public readonly union Element(Template, Menu, ObjectLiteral) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).Visit(visitor);

    public void VisitChildren(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).VisitChildren(visitor);
}
