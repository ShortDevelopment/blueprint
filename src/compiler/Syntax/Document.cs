namespace BluePrint.Compiler.Syntax;

public sealed record Document
{
    public required IReadOnlyList<Using> Usings { get; init; }
    public required string? TranslationDomain { get; init; }
    public required IReadOnlyList<Element> Elements { get; init; }
}

public sealed record Using(string Namespace, double Version);

public closed record Element;
