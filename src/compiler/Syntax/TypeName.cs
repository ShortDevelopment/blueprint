namespace BluePrint.Compiler.Syntax;

public sealed record TypeName(bool IsExternal, IReadOnlyList<string> Parts) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitTypeName(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }

    public override string ToString()
        => $"{(IsExternal ? "$" : "")}{string.Join('.', Parts)}";
}
